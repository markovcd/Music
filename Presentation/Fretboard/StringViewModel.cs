// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Presentation.Utility;

namespace Presentation.Fretboard;

public class StringViewModel : BindableBase<StringViewModel>
{
  [CoupledWith(nameof(ZeroFret))]
  public IBindable<IEnumerable<FretViewModel>> Frets { get; init; }

  public FretViewModel ZeroFret => Frets.Value?.FirstOrDefault() ?? throw new InvalidOperationException();

  public StringViewModel()
  {
    RegisterProperties();
  }

  public void Initialize(IEnumerable<FretViewModel> frets)
  {
    Frets.Value = frets.ToImmutableList();
  }
}