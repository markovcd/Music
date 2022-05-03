using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Presentation.Utility;

public abstract class BindableBase<TViewModel> : INotifyPropertyChanged
  where TViewModel : BindableBase<TViewModel>
{
  public event PropertyChangedEventHandler? PropertyChanged;

  protected BindableBase()
  {
    if (GetType() != typeof(TViewModel)) 
      throw new InvalidOperationException("Invalid generic type argument");
  }
  
  private Bindable<TProperty> CreateBindableProperty<TProperty>(string propertyName, IEnumerable<string> coupledPropertyNames)
  {
    return new Bindable<TProperty>(propertyName, OnPropertyChanged, coupledPropertyNames);
  }
  
  private object CreateBindableProperty(string propertyName, IEnumerable<string> coupledPropertyNames, Type propertyType)
  {
    var bindableType = typeof(BindableBase<>.Bindable<>).MakeGenericType(typeof(TViewModel), propertyType);
    var constructor = bindableType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
    var onPropertyChanged = OnPropertyChanged;
    
    return constructor.Invoke(new object[] { propertyName, onPropertyChanged, coupledPropertyNames })
           ?? throw new InvalidOperationException();
  }

  protected void RegisterProperties()
  {
    foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
    {
      if (!propertyInfo.PropertyType.IsGenericType) continue;
      if (propertyInfo.PropertyType.GetGenericTypeDefinition() != typeof(IBindable<>)) continue;

      var propertyType = propertyInfo.PropertyType.GetGenericArguments()[0];

      var coupledPropertyNames = GetCoupledPropertyNames(propertyInfo);
      var bindableProperty = CreateBindableProperty(propertyInfo.Name, coupledPropertyNames, propertyType);
      
      propertyInfo.SetValue(this, bindableProperty);
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
    var coupledPropertyNames = GetCoupledPropertyNames(propertyInfo);
    var bindableProperty = CreateBindableProperty<TProperty>(propertyInfo.Name, coupledPropertyNames);

    propertyInfo.SetValue(this, bindableProperty);
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

  public IDisposable ListenForChange<TProperty>(
    Expression<Func<TViewModel, IBindable<TProperty>>> propertyExpression,
    Action<TViewModel, TProperty?> callback)
  {
    var propertyInfo = GetPropertyInfo(propertyExpression);
    
    void Handler(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == propertyInfo.Name) callback(
        (TViewModel)this,
        ((IBindable<TProperty>)propertyInfo.GetValue(this)!).Value);
    }

    PropertyChanged += Handler;

    void Unsubscribe() => PropertyChanged -= Handler;

    return new DisposableAction(Unsubscribe);
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
    private readonly string propertyName;
    private readonly IEnumerable<string> coupledPropertyNames;
    private readonly Action<string> propertyChangedNotification;
    private T? value;

    public T? Value
    {
      get => value;
      set => SetProperty(value);
    }
  
    internal Bindable(
      string propertyName, 
      Action<string> propertyChangedNotification,
      IEnumerable<string> coupledPropertyNames)
    {
      this.propertyName = propertyName;
      this.propertyChangedNotification = propertyChangedNotification;
      this.coupledPropertyNames = coupledPropertyNames;
    }

    public void NotifyChange()
    {
      propertyChangedNotification(propertyName);
      foreach (var coupledPropertyName in coupledPropertyNames)
        propertyChangedNotification(coupledPropertyName);
    }

    // ReSharper disable once ParameterHidesMember
    private void SetProperty(T? value)
    {
      if (Equals(value)) return;
    
      this.value = value;
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
