// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BadgerMvvm.Core;
using Domain;
using DomainFretboard = Domain.Fretboard;

namespace Presentation.Fretboard;

public sealed class FretboardViewModel : BindableBase<FretboardViewModel>
{
  private const int DefaultFretCount = DomainFretboard.DefaultFretCount;
  private const int MaxShapes = 12;

  private static readonly IReadOnlyList<string> RootNames = new[]
  {
    "C", "C#", "Db", "D", "D#", "Eb", "E", "F", "F#", "Gb", "G", "G#", "Ab", "A", "A#", "Bb", "B",
  };

  private readonly DelegateCommand transposeUp;
  private readonly DelegateCommand transposeDown;
  private readonly DelegateCommand clearFrets;
  private ImmutableList<Pitch> tunings = ImmutableList<Pitch>.Empty;
  private int fretCount = DefaultFretCount;
  private NoteNames? keyNames;
  private IReadOnlyDictionary<string, DomainFretboard> foundShapes = new Dictionary<string, DomainFretboard>();
  private bool isApplyingFrets;

  public IBindable<IEnumerable<StringViewModel>> Strings { get; init; }

  /// <summary>
  /// Names of the chord formed by the checked frets, best first and separated by commas (for example "C6, Am7/C").
  /// Empty when the checked frets do not form a known chord. While a scale is shown, notes of the scale are spelled
  /// the way the scale spells them.
  /// </summary>
  public IBindable<string> ChordNames { get; init; }

  /// <summary>The tunings to choose from.</summary>
  public IReadOnlyList<NamedTuning> Tunings => TuningTemplates.All;

  /// <summary>The tuning of the strings. Choosing another one replaces the strings and unchecks every fret.</summary>
  public IBindable<NamedTuning> SelectedTuning { get; init; }

  /// <summary>The roots to choose from for the scale.</summary>
  public IReadOnlyList<string> Roots => RootNames;

  /// <summary>The scales to choose from.</summary>
  public IReadOnlyList<NamedScale> Scales => ScaleTemplates.All;

  public IBindable<string> SelectedRoot { get; init; }

  public IBindable<NamedScale> SelectedScale { get; init; }

  /// <summary>When true the notes of the selected scale are marked on the strings.</summary>
  public IBindable<bool> ShowScale { get; init; }

  /// <summary>The notes of the shown scale, for example "Db Eb F Gb Ab Bb C". Empty while no scale is shown.</summary>
  public IBindable<string> ScaleNotes { get; init; }

  /// <summary>The triads and seventh chords of the shown scale, one line each. Empty while no scale is shown.</summary>
  public IBindable<string> ScaleChords { get; init; }

  /// <summary>Moves every checked fret one fret up the neck. Not available if one is already on the last fret.</summary>
  public ICommand TransposeUp => transposeUp;

  /// <summary>Moves every checked fret one fret towards the nut. Not available if one is already on the open string.</summary>
  public ICommand TransposeDown => transposeDown;

  /// <summary>Unchecks every fret.</summary>
  public ICommand ClearFrets => clearFrets;

  /// <summary>The chord to look for, for example "Am7" or "C/G".</summary>
  public IBindable<string> ChordQuery { get; init; }

  /// <summary>"Not a chord", "No shapes found" or how many shapes were found for <see cref="ChordQuery"/>.</summary>
  public IBindable<string> ChordQueryStatus { get; init; }

  /// <summary>The shapes found for <see cref="ChordQuery"/>, easiest first, written like "x 3 2 0 1 0".</summary>
  public IBindable<IEnumerable<string>> Shapes { get; init; }

  /// <summary>Choosing a shape checks its frets on the strings.</summary>
  public IBindable<string> SelectedShape { get; init; }

  public FretboardViewModel()
  {
    RegisterProperties();

    transposeUp = Command(() => Transpose(1));
    transposeDown = Command(() => Transpose(-1));
    clearFrets = Command(() => ApplyFretboard(new DomainFretboard(tunings, fretCount)));

    SetUp();
  }

  private static DelegateCommand Command(System.Action action)
  {
    return new DelegateCommand(() =>
    {
      action();
      return Task.CompletedTask;
    });
  }

  private void SetUp()
  {
    SelectedTuning.Value = TuningTemplates.Standard;
    SelectedRoot.Value = RootNames[0];
    SelectedScale.Value = Scales[0];
    ShowScale.Value = false;
    ChordQuery.Value = string.Empty;

    SelectedTuning.ListenForChange(_ => ApplyInstrument());
    ShowScale.ListenForChange(_ => UpdateScale());
    SelectedRoot.ListenForChange(_ => UpdateScale());
    SelectedScale.ListenForChange(_ => UpdateScale());
    ChordQuery.ListenForChange(_ => UpdateShapes());
    SelectedShape.ListenForChange(_ => ApplySelectedShape());

    ApplyInstrument();
  }

  private void ApplyInstrument()
  {
    Initialize(SelectedTuning.Value?.Pitches ?? TuningTemplates.Standard.Pitches);
  }

  public void Initialize(IEnumerable<Pitch> stringTunings, int fretCount = DefaultFretCount)
  {
    tunings = stringTunings.ToImmutableList();
    this.fretCount = fretCount;

    var strings = tunings.Select(p => StringViewModel.FromPitch(p, fretCount))
      .ToImmutableList();

    foreach (var fret in strings.SelectMany(s => s.Frets.Value!))
      fret.IsChecked.ListenForChange(_ => OnFretChecked());

    Strings.Value = strings;

    UpdateScale();
    UpdateShapes();
  }

  private IEnumerable<FretViewModel> AllFrets => Strings.Value!.SelectMany(s => s.Frets.Value!);

  private void OnFretChecked()
  {
    if (isApplyingFrets) return;

    UpdateChordNames();
    UpdateCommands();
  }

  private DomainFretboard BuildFretboard()
  {
    var board = new DomainFretboard(tunings, fretCount);
    var strings = Strings.Value!.ToList();

    for (var i = 0; i < strings.Count; i++)
    {
      foreach (var fret in strings[i].Frets.Value!.Where(f => f.IsChecked.Value))
        board = board.PressFret(i, fret.Fret);
    }

    return board;
  }

  /// <summary>Checks exactly the frets that are pressed on the board.</summary>
  private void ApplyFretboard(DomainFretboard board)
  {
    isApplyingFrets = true;

    try
    {
      var strings = Strings.Value!.ToList();

      for (var i = 0; i < strings.Count; i++)
      {
        foreach (var fret in strings[i].Frets.Value!)
          fret.IsChecked.Value = board.IsPressed(i, fret.Fret);
      }
    }
    finally
    {
      isApplyingFrets = false;
    }

    UpdateChordNames();
    UpdateCommands();
  }

  private void UpdateChordNames()
  {
    if (tunings.IsEmpty)
    {
      ChordNames.Value = string.Empty;
      return;
    }

    ChordNames.Value = string.Join(", ", BuildFretboard().GetChordNames(keyNames));
  }

  private void Transpose(int frets)
  {
    if (!CanTranspose(frets)) return;

    ApplyFretboard(BuildFretboard().Transpose(frets));
  }

  private bool CanTranspose(int frets)
  {
    var checkedFrets = AllFrets.Where(f => f.IsChecked.Value).Select(f => (int)f.Fret).ToList();

    return checkedFrets.Count > 0
           && checkedFrets.Min() + frets >= 0
           && checkedFrets.Max() + frets < fretCount;
  }

  private void UpdateCommands()
  {
    transposeUp.CanExecute = CanTranspose(1);
    transposeDown.CanExecute = CanTranspose(-1);
  }

  private void UpdateScale()
  {
    if (Strings.Value is null) return;

    if (ShowScale.Value && SelectedScale.Value is { } selected)
    {
      var rootName = SelectedRoot.Value ?? RootNames[0];
      var root = Note.Parse(rootName);

      keyNames = selected.Scale.GetNoteNames(root, rootName);

      foreach (var fret in AllFrets)
        fret.SetScale(keyNames.Find(fret.Pitch.Note), fret.Pitch.Note == root);

      ScaleNotes.Value = keyNames.ToString();
      ScaleChords.Value = DescribeChords(selected.Scale, root, rootName);
    }
    else
    {
      keyNames = null;

      foreach (var fret in AllFrets)
        fret.SetScale(null, false);

      ScaleNotes.Value = string.Empty;
      ScaleChords.Value = string.Empty;
    }

    UpdateChordNames();
    UpdateCommands();
  }

  private static string DescribeChords(Scale scale, Note root, string rootName)
  {
    if (scale.Intervals.Count() != 7) return string.Empty;

    string Line(string title, IReadOnlyList<Degree> template)
    {
      var names = scale.GetChordNames(root, template, rootName).Select(n => n?.ToString() ?? "-");
      return $"{title}: {string.Join("  ", names)}";
    }

    return $"{Line("Triads", ChordTemplates.Triad)}\n{Line("Sevenths", ChordTemplates.Seventh)}";
  }

  private void UpdateShapes()
  {
    SelectedShape.Value = null;

    var found = new Dictionary<string, DomainFretboard>();
    var text = ChordQuery.Value?.Trim() ?? string.Empty;

    if (text.Length == 0 || tunings.IsEmpty)
    {
      ChordQueryStatus.Value = string.Empty;
    }
    else if (!ChordName.TryParse(text, out var chord))
    {
      ChordQueryStatus.Value = "Not a chord";
    }
    else
    {
      foreach (var shape in ChordShapeFinder.Find(new DomainFretboard(tunings, fretCount), chord, maxResults: MaxShapes))
        found[shape.Diagram] = shape;

      ChordQueryStatus.Value = found.Count == 0 ? "No shapes found" : $"{found.Count} shapes";
    }

    foundShapes = found;
    Shapes.Value = found.Keys.ToImmutableList();
  }

  private void ApplySelectedShape()
  {
    if (SelectedShape.Value is { } diagram && foundShapes.TryGetValue(diagram, out var shape))
      ApplyFretboard(shape);
  }
}
