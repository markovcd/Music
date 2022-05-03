// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Domain;
using Presentation.Utility;

namespace Presentation.Fretboard;

public class StringViewModel : BindableBase<StringViewModel>
{
  [CoupledWith(nameof(ZeroFret))]
  public IBindable<IEnumerable<FretViewModel>> Frets { get; init; }

  public FretViewModel ZeroFret => Frets.Value!.Single(f => f.IsZero.Value);

  public StringViewModel()
  {
    RegisterProperties();
  }

  internal static StringViewModel FromPitch(Pitch tuning, int fretCount)
  {
    var @string = new StringViewModel();
    @string.Initialize(tuning, fretCount);
    return @string;
  }
  
  internal void Initialize(Pitch tuning, int fretCount)
  {
    var frets = Enumerable.Range(0, fretCount)
      .Select(i => new Interval(i))
      .Select(i => tuning + i)
      .Select(FretViewModel.FromPitch)
      .ToImmutableList();

    frets[0].IsZero.Value = true;

    Frets.Value = frets;
  }
}
