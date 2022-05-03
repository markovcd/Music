using Presentation.Utility;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Presentation.Fretboard;

public class Vm : BindableBase<Vm>
{
  public IBindable<string> Title { get; init; }

  public Vm()
  {
    RegisterProperties();
  }
}
