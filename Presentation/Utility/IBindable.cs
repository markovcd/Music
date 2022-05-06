using System;
using System.Collections.Generic;

namespace Presentation.Utility;

public interface IBindable
{
  string Name { get; }
  
  bool HasErrors { get; }
  
  IReadOnlyList<string> Errors { get; }
  
  void AddError(string message);
  
  void ClearErrors();
}

public interface IBindable<T> : IBindable, IEquatable<T>, IEquatable<IBindable<T>>
{
  T? Value { get; set; }

  IDisposable ListenForChange(Action<IBindable<T>> callback);
  
  IDisposable ListenForErrors(Action<IBindable<T>> callback);

  IDisposable AddValidationRule(Func<IBindable<T>, ValidationResult> rule);
}
