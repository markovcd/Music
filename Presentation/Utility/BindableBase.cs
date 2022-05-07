using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Presentation.Utility;

public abstract partial class BindableBase<TViewModel> : INotifyPropertyChanged, INotifyDataErrorInfo
  where TViewModel : BindableBase<TViewModel>
{
  private readonly ThreadSafeList<IBindable> registeredProperties = new();
  
  public event PropertyChangedEventHandler? PropertyChanged;
  
  public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

  public bool HasErrors => RegisteredProperties.Any(p => p.HasErrors);

  protected IEnumerable<IBindable> RegisteredProperties => registeredProperties;

  protected BindableBase()
  {
    if (GetType() != typeof(TViewModel)) 
      throw new InvalidOperationException("Invalid generic type argument");
  }

  protected void ClearErrors()
  {
    foreach (var registeredProperty in RegisteredProperties)
    {
      registeredProperty.ClearErrors();
    }
  }
  
  protected void Validate()
  {
    foreach (var registeredProperty in RegisteredProperties)
    {
      registeredProperty.Validate();
    }
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
  
  IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName)
  {
    if (propertyName == null) return RegisteredProperties.SelectMany(p => p.Errors);
    var property = RegisteredProperties.SingleOrDefault(p => p.Name == propertyName);
    return property?.Errors ?? Enumerable.Empty<string>();
  }
  
  private void OnPropertyChanged(string propertyName)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
  
  private void OnErrorsChanged(string propertyName)
  {
    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
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
  
  private static IEnumerable<string> GetCoupledPropertyNames(MemberInfo memberInfo)
  {
    return memberInfo.GetCustomAttributes(typeof(CoupledWithAttribute))
      .Cast<CoupledWithAttribute>()
      .Select(c => c.PropertyName)
      .Distinct()
      .ToImmutableList();
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
}
