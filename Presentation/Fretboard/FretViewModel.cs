// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

using Presentation.Utility;

namespace Presentation.Fretboard;

public class FretViewModel : BindableBase<FretViewModel>
{
  public IBindable<string> Caption { get; init; }
  
  public IBindable<bool> IsChecked { get; init; }
  
  public IBindable<bool> IsZero { get; init; }

  public FretViewModel()
  {
    RegisterProperties();
    
    Initialize("dupa");
  }

  public void Initialize(string caption)
  {
    Caption.Value = caption;
  }
}
