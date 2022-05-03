using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Presentation.Utility;

public abstract class BindableBase<TViewModel> : INotifyPropertyChanged
  where TViewModel : BindableBase<TViewModel>
{
  public event PropertyChangedEventHandler? PropertyChanged;

  private Bindable<TProperty> CreateBindableProperty<TProperty>(string propertyName)
  {
    return new Bindable<TProperty>(propertyName, OnPropertyChanged);
  }
  
  private object CreateBindableProperty(string propertyName, Type propertyType)
  {
    var bindableType = typeof(BindableBase<>.Bindable<>).MakeGenericType(typeof(TViewModel), propertyType);
    var constructor = bindableType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
    
    return constructor.Invoke(new object[] { propertyName, OnPropertyChanged})
           ?? throw new InvalidOperationException();
  }

  protected void RegisterProperties()
  {
    foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
    {
      if (!propertyInfo.PropertyType.IsGenericType) continue;
      if (propertyInfo.PropertyType.GetGenericTypeDefinition() != typeof(IBindable<>)) continue;

      var propertyType = propertyInfo.PropertyType.GetGenericArguments()[0];
      var bindableProperty = CreateBindableProperty(propertyInfo.Name, propertyType);
      
      propertyInfo.SetValue(this, bindableProperty);
    }
  }
  
  protected void RegisterProperty<TProperty>(
    Expression<Func<TViewModel, IBindable<TProperty>>> propertyExpression)
  {
    var propertyInfo = GetPropertyInfo(propertyExpression);

    var bindableProperty = CreateBindableProperty<TProperty>(propertyInfo.Name);

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

  private sealed class Bindable<T> : IBindable<T>
  {
    private readonly string propertyName;
    private readonly Action<string> propertyChangedNotification;
    private T? value;

    public T? Value
    {
      get => value;
      set => SetProperty(value);
    }
  
    internal Bindable(string propertyName, Action<string> propertyChangedNotification)
    {
      this.propertyName = propertyName;
      this.propertyChangedNotification = propertyChangedNotification;
    }

    public void NotifyChange()
    {
      propertyChangedNotification(propertyName);
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
