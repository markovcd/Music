using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Presentation.Utility;

public abstract partial class BindableBase<TViewModel>
{
  private sealed class Bindable<T> : IBindable<T>
  {
    private readonly IEnumerable<string> coupledPropertyNames;
    private readonly Action<string> propertyChangedNotification;
    private readonly Action<string> errorsChangedNotification;
    private readonly ThreadSafeList<Action<IBindable<T>>> changedCallbacks = new();
    private readonly ThreadSafeList<Action<IBindable<T>>> errorCallbacks = new();
    private readonly ThreadSafeList<IValidationRule<T>> validationRules = new();
    private readonly ThreadSafeList<string> errors = new();

    private T? value;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    
    public string Name { get; }

    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName)
    {
      return Errors;
    }

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
      return AddValidationRule(new DelegateValidationRule(rule));
    }

    public IDisposable AddValidationRule(IValidationRule<T> rule)
    {
      validationRules.Add(rule);
      
      void Unsubscribe() => validationRules.Remove(rule);

      return new DisposableAction(Unsubscribe);
    }

    public void Validate()
    {
      errors.Clear();
      
      var validationResults = validationRules.Select(r => r.Validate(this))
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

      OnErrorsChanged();
    }

    private void NotifyChange()
    {
      propertyChangedNotification(Name);
      
      foreach (var coupledPropertyName in coupledPropertyNames)
        propertyChangedNotification(coupledPropertyName);

      foreach (var callback in changedCallbacks)
        callback(this);

      OnPropertyChanged();
    }

    // ReSharper disable once ParameterHidesMember
    private void SetProperty(T? value)
    {
      if (Equals(value)) return;
    
      this.value = value;
      Validate();
      NotifyChange();
    }
    
    private void OnErrorsChanged()
    {
      ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));
    }

    private void OnPropertyChanged()
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
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

    private sealed class DelegateValidationRule : IValidationRule<T>
    {
      private readonly Func<IBindable<T>, ValidationResult> func;

      public DelegateValidationRule(Func<IBindable<T>, ValidationResult> func)
      {
        this.func = func;
      }
      
      public ValidationResult Validate(IBindable<T> property)
      {
        return func(property);
      }
    }
  }
}
