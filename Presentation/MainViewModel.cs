using Presentation.Fretboard;

namespace Presentation;

public class MainViewModel
{
  public FretboardViewModel FretboardViewModel { get; }
  
  public MainViewModel()
  {
    FretboardViewModel = new FretboardViewModel();
  }
}
