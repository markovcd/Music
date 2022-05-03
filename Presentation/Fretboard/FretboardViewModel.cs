// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Domain;
using Presentation.Utility;

namespace Presentation.Fretboard;

public sealed class FretboardViewModel : BindableBase<FretboardViewModel>
{
  private const int DefaultFretCount = 24;
  
  public IBindable<IEnumerable<StringViewModel>> Strings { get; init; }

  public FretboardViewModel()
  {
    RegisterProperties();

    var stringTunings = new[]
    {
      new Pitch(new Octave(4), Note.Parse("E")),
      new Pitch(new Octave(3), Note.Parse("B")),
      new Pitch(new Octave(3), Note.Parse("G")),
      new Pitch(new Octave(3), Note.Parse("D")),
      new Pitch(new Octave(2), Note.Parse("A")),
      new Pitch(new Octave(2), Note.Parse("E"))
    };
    
    Initialize(stringTunings);
  }
  
  public void Initialize(IEnumerable<Pitch> stringTunings, int fretCount = DefaultFretCount)
  {
    Strings.Value = stringTunings.Select(p => StringViewModel.FromPitch(p, fretCount))
      .ToImmutableList();
  }
}
