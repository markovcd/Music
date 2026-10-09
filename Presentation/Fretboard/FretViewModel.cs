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

  /// <summary>The fret number, where 0 is the open string.</summary>
  public Interval Fret => interval;

  /// <summary>The pitch that sounds when this fret is played.</summary>
  public Pitch Pitch => pitch;

  public IBindable<string> Caption { get; init; }
  
  public IBindable<bool> IsChecked { get; init; }
  
  public IBindable<bool> IsZero { get; init; }

  /// <summary>True if the note of this fret is in the scale that is shown.</summary>
  public IBindable<bool> IsInScale { get; init; }

  /// <summary>True if the note of this fret is the root of the scale that is shown.</summary>
  public IBindable<bool> IsScaleRoot { get; init; }

  /// <summary>The label of the note in the chord that is shown ("R", "3", "b7" ...), or empty if the note is not in it.</summary>
  public IBindable<string> ChordToneLabel { get; init; }

  /// <summary>True if the note of this fret is part of the chord that is shown.</summary>
  public IBindable<bool> IsChordTone { get; init; }

  /// <summary>True if the note of this fret is the root of the chord that is shown.</summary>
  public IBindable<bool> IsChordRoot { get; init; }

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
  
  /// <summary>
  /// Marks the fret as part of the scale that is shown and names its note the way the scale does.
  /// Pass null if the note is not in the scale, which shows the fret as usual.
  /// </summary>
  public void SetScale(string? noteName, bool isRoot)
  {
    IsInScale.Value = noteName is not null;
    IsScaleRoot.Value = noteName is not null && isRoot;
    Caption.Value = noteName is null ? pitch.ToString() : $"{noteName}{pitch.Octave}";
  }

  /// <summary>
  /// Marks the fret as part of the chord that is shown, with the label the note has in the chord.
  /// Pass null if the note is not in the chord.
  /// </summary>
  public void SetChordTone(string? label)
  {
    IsChordTone.Value = label is not null;
    IsChordRoot.Value = label == "R";
    ChordToneLabel.Value = label ?? string.Empty;
  }

  internal static FretViewModel Create(Pitch pitch, Interval interval)
  {
    var fret = new FretViewModel();
    fret.Initialize(pitch, interval);
    return fret;
  }
}
