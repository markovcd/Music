// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

using Domain;
using Presentation.Utility;

namespace Presentation.Fretboard;

public class FretViewModel : BindableBase<FretViewModel>
{
  private Pitch pitch;
  
  public IBindable<string> Caption { get; init; }
  
  public IBindable<bool> IsChecked { get; init; }
  
  public IBindable<bool> IsZero { get; init; }

  public FretViewModel()
  {
    RegisterProperties();
  }
  
  // ReSharper disable once ParameterHidesMember
  public void Initialize(Pitch pitch)
  {
    this.pitch = pitch;
    Caption.Value = pitch.ToString();
    ListenForChange(vm => IsChecked, IsCheckedChanged);
  }
  
  internal static FretViewModel FromPitch(Pitch pitch)
  {
    var fret = new FretViewModel();
    fret.Initialize(pitch);
    return fret;
  }

  private void IsCheckedChanged()
  {
    
  }
}
