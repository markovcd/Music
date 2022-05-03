// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

using System.Collections.Generic;
using System.Collections.Immutable;
using Presentation.Utility;

namespace Presentation.Fretboard;

public class FretboardViewModel : BindableBase<FretboardViewModel>
{
  public IBindable<IEnumerable<StringViewModel>> Strings { get; init; }

  public FretboardViewModel()
  {
    RegisterProperties();
  }
  
  public void Initialize(IEnumerable<StringViewModel> strings)
  {
    Strings.Value = strings.ToImmutableList();
  }
}
