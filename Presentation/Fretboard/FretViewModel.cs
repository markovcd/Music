// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ParameterHidesMember
#pragma warning disable CS8618

using BadgerMvvm.Core;
using Domain;

namespace Presentation.Fretboard;

public sealed class FretViewModel : BindableBase<FretViewModel>
{
  private Pitch pitch;
  private Interval interval;

  public IBindable<string> Caption { get; init; }
  
  public IBindable<bool> IsChecked { get; init; }
  
  public IBindable<bool> IsZero { get; init; }

  public FretViewModel()
  {
    RegisterProperties();
  }
  
  public void Initialize(Pitch pitch, Interval interval)
  {
    this.pitch = pitch;
    this.interval = interval;
    Caption.Value = pitch.ToString();
    IsZero.Value = interval == Interval.Tonic;
  }
  
  internal static FretViewModel Create(Pitch pitch, Interval interval)
  {
    var fret = new FretViewModel();
    fret.Initialize(pitch, interval);
    return fret;
  }
}
