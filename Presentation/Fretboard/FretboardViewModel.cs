// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BadgerMvvm.Core;
using Domain;
using DomainFretboard = Domain.Fretboard;

namespace Presentation.Fretboard;

public sealed class FretboardViewModel : BindableBase<FretboardViewModel>
{
  private const int DefaultFretCount = DomainFretboard.DefaultFretCount;

  private ImmutableList<Pitch> tunings = ImmutableList<Pitch>.Empty;
  private int fretCount = DefaultFretCount;

  public IBindable<IEnumerable<StringViewModel>> Strings { get; init; }

  /// <summary>
  /// Names of the chord formed by the checked frets, best first and separated by commas (for example "C6, Am7/C").
  /// Empty when the checked frets do not form a known chord.
  /// </summary>
  public IBindable<string> ChordNames { get; init; }

  public FretboardViewModel()
  {
    RegisterProperties();

    Initialize(DomainFretboard.StandardTuning);
  }

  public void Initialize(IEnumerable<Pitch> stringTunings, int fretCount = DefaultFretCount)
  {
    tunings = stringTunings.ToImmutableList();
    this.fretCount = fretCount;

    var strings = tunings.Select(p => StringViewModel.FromPitch(p, fretCount))
      .ToImmutableList();

    foreach (var fret in strings.SelectMany(s => s.Frets.Value!))
      fret.IsChecked.ListenForChange(_ => UpdateChordNames());

    Strings.Value = strings;
    UpdateChordNames();
  }

  private void UpdateChordNames()
  {
    if (tunings.IsEmpty)
    {
      ChordNames.Value = string.Empty;
      return;
    }

    var board = new DomainFretboard(tunings, fretCount);
    var strings = Strings.Value!.ToList();

    for (var i = 0; i < strings.Count; i++)
    {
      foreach (var fret in strings[i].Frets.Value!.Where(f => f.IsChecked.Value))
        board = board.PressFret(i, fret.Fret);
    }

    ChordNames.Value = string.Join(", ", board.GetChordNames());
  }
}
