using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Presentation.Utility;

public abstract class BindableBase<TViewModel> : INotifyPropertyChanged, INotifyDataErrorInfo
  where TViewModel : BindableBase<TViewModel>
{
  private readonly List<IBindable> registeredProperties = new();
  
  public event PropertyChangedEventHandler? PropertyChanged;
  
  public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

  public bool HasErrors => RegisteredProperties.Any(p => p.HasErrors);

  protected IReadOnlyList<IBindable> RegisteredProperties => registeredProperties;

  protected BindableBase()
  {
    if (GetType() != typeof(TViewModel)) 
      throw new InvalidOperationException("Invalid generic type argument");
  }
  
  private Bindable<TProperty> CreateBindableProperty<TProperty>(string propertyName, IEnumerable<string> coupledPropertyNames)
  {
    return new Bindable<TProperty>(
      propertyName, 
      OnPropertyChanged,
      OnErrorsChanged,
      coupledPropertyNames);
  }
  
  private IBindable CreateBindableProperty(string propertyName, IEnumerable<string> coupledPropertyNames, Type propertyType)
  {
    var bindableType = typeof(BindableBase<>.Bindable<>).MakeGenericType(typeof(TViewModel), propertyType);
    var constructor = bindableType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
    var onPropertyChanged = OnPropertyChanged;
    var onErrorsChanged = OnErrorsChanged;
    
    return (IBindable)constructor.Invoke(
             new object[] { propertyName, onPropertyChanged, onErrorsChanged, coupledPropertyNames })
           ?? throw new InvalidOperationException();
  }

  protected void RegisterProperties()
  {
    foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
    {
      if (!propertyInfo.PropertyType.IsGenericType) continue;
      if (propertyInfo.PropertyType.GetGenericTypeDefinition() != typeof(IBindable<>)) continue;
      
      if (propertyInfo.GetValue(this) != null)
        throw new InvalidOperationException("Property already registered");
      
      var propertyType = propertyInfo.PropertyType.GetGenericArguments()[0];

      var coupledPropertyNames = GetCoupledPropertyNames(propertyInfo);
      var bindableProperty = CreateBindableProperty(propertyInfo.Name, coupledPropertyNames, propertyType);

      propertyInfo.SetValue(this, bindableProperty);
      
      registeredProperties.Add(bindableProperty);
    }
  }

  private static IEnumerable<string> GetCoupledPropertyNames(MemberInfo memberInfo)
  {
    return memberInfo.GetCustomAttributes(typeof(CoupledWithAttribute))
      .Cast<CoupledWithAttribute>()
      .Select(c => c.PropertyName)
      .ToImmutableList();
  }
  
  protected void RegisterProperty<TProperty>(
    Expression<Func<TViewModel, IBindable<TProperty>>> propertyExpression)
  {
    var propertyInfo = GetPropertyInfo(propertyExpression);
    
    if (propertyInfo.GetValue(this) != null)
      throw new InvalidOperationException("Property already registered");
    
    var coupledPropertyNames = GetCoupledPropertyNames(propertyInfo);
    var bindableProperty = CreateBindableProperty<TProperty>(propertyInfo.Name, coupledPropertyNames);

    if (propertyInfo.GetValue(this) != null) throw new InvalidOperationException("Property already registered");
    
    propertyInfo.SetValue(this, bindableProperty);
    
    registeredProperties.Add(bindableProperty);
  }
  
  private static PropertyInfo GetPropertyInfo<TProperty>(
    Expression<Func<TViewModel, TProperty>> propertyExpression)
  {
    if (propertyExpression.Body is not MemberExpression member)
      throw new ArgumentException($"Expression '{propertyExpression}' refers to a method, not a property");

    var propertyInfo = member.Member as PropertyInfo;
    
    if (propertyInfo == null)
      throw new ArgumentException($"Expression '{propertyExpression}' refers to a field, not a property");
    
    return propertyInfo;
  }
  
  private void OnPropertyChanged(string propertyName)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
  
  private void OnErrorsChanged(string propertyName)
  {
    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
  }

  IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName)
  {
    if (propertyName == null) return RegisteredProperties.SelectMany(p => p.Errors);
    var property = RegisteredProperties.SingleOrDefault(p => p.Name == propertyName);
    return property?.Errors ?? Enumerable.Empty<string>();
  }
  
  private sealed class DisposableAction : IDisposable
  {
    private readonly Action action;
    
    public DisposableAction(Action action)
    {
      this.action = action;
    }

    public void Dispose()
    {
      action();
    }
  }

  private sealed class Bindable<T> : IBindable<T>
  {
    private readonly IEnumerable<string> coupledPropertyNames;
    private readonly Action<string> propertyChangedNotification;
    private readonly Action<string> errorsChangedNotification;
    private T? value;
    private readonly List<Action<IBindable<T>>> changedCallbacks = new();
    private readonly List<Action<IBindable<T>>> errorCallbacks = new();
    private readonly List<Func<IBindable<T>, ValidationResult>> validationRules = new();

    private readonly List<string> errors = new();

    public string Name { get; }

    public bool HasErrors => Errors.Any();
    
    public IReadOnlyList<string> Errors => errors;

    public T? Value
    {
      get => value;
      set => SetProperty(value);
    }
  
    internal Bindable(
      string name, 
      Action<string> propertyChangedNotification,
      Action<string> errorsChangedNotification,
      IEnumerable<string> coupledPropertyNames)
    {
      Name = name;
      this.propertyChangedNotification = propertyChangedNotification;
      this.errorsChangedNotification = errorsChangedNotification;
      this.coupledPropertyNames = coupledPropertyNames;
    }
    
    public IDisposable ListenForChange(Action<IBindable<T>> callback)
    {
      changedCallbacks.Add(callback);

      void Unsubscribe() => changedCallbacks.Remove(callback);

      return new DisposableAction(Unsubscribe);
    }
    
    public IDisposable ListenForErrors(Action<IBindable<T>> callback)
    {
      errorCallbacks.Add(callback);

      void Unsubscribe() => errorCallbacks.Remove(callback);

      return new DisposableAction(Unsubscribe);
    }

    public IDisposable AddValidationRule(Func<IBindable<T>, ValidationResult> rule)
    {
      validationRules.Add(rule);
      
      void Unsubscribe() => validationRules.Remove(rule);

      return new DisposableAction(Unsubscribe);
    }

    private void ValidateRules()
    {
      errors.Clear();
      
      var validationResults = validationRules.Select(r => r(this))
        .Where(v => !v.IsSuccess)
        .Select(v => v.Message);
      
      errors.AddRange(validationResults);

      NotifyError();
    }

    public void AddError(string message)
    {
      errors.Add(message);

      NotifyError();
    }

    public void ClearErrors()
    {
      errors.Clear();

      NotifyError();
    }
    
    private void NotifyError()
    {
      errorsChangedNotification(Name);
      
      foreach (var callback in errorCallbacks)
        callback(this);
    }

    private void NotifyChange()
    {
      propertyChangedNotification(Name);
      
      foreach (var coupledPropertyName in coupledPropertyNames)
        propertyChangedNotification(coupledPropertyName);

      foreach (var callback in changedCallbacks)
        callback(this);
    }

    // ReSharper disable once ParameterHidesMember
    private void SetProperty(T? value)
    {
      if (Equals(value)) return;
    
      this.value = value;
      ValidateRules();
      NotifyChange();
    }
    
    public bool Equals(T? other)
    {
      return Value != null && Value.Equals(other);
    }

    public bool Equals(IBindable<T>? other)
    {
      return other != null && Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
      return obj switch
      {
        IBindable<T> otherBindable => Equals(otherBindable),
        T otherT => Equals(otherT),
        _ => false
      };
    }

    public override int GetHashCode()
    {
      return Value?.GetHashCode() ?? default;
    }

    public override string? ToString()
    {
      return Value?.ToString();
    }

    public static bool operator ==(Bindable<T> first, Bindable<T> second)
    {
      return first.Equals(second);
    }
    
    public static bool operator ==(Bindable<T> first, T second)
    {
      return first.Equals(second);
    }
    
    public static bool operator ==(T first, Bindable<T> second)
    {
      return second.Equals(first);
    }
    
    public static bool operator !=(Bindable<T> first, Bindable<T> second)
    {
      return !first.Equals(second);
    }
    
    public static bool operator !=(Bindable<T> first, T second)
    {
      return !first.Equals(second);
    }
    
    public static bool operator !=(T first, Bindable<T> second)
    {
      return !second.Equals(first);
    }
  }
}
