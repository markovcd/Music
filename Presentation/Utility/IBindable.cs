using System;

namespace Presentation.Utility;

public interface IBindable<T> : IEquatable<T>, IEquatable<IBindable<T>>
{
  T? Value { get; set; }

  void NotifyChange();

}
