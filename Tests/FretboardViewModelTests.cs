using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Domain;
using FluentAssertions;
using NUnit.Framework;
using Presentation.Fretboard;

namespace Tests;

[TestFixture]
public class FretboardViewModelTests
{
    [Test]
    public void XX()
    {
        var x = new FretViewModel();
        var l = new List<string>();
        x.PropertyChanged += (_, p) => l.Add(p.PropertyName!);

        x.IsChecked.Value = true;
    }

    private static Pitch P(string note, int octave)
    {
        return new Pitch(new Octave(octave), Note.Parse(note));
    }

    private static FretViewModel Fret(FretboardViewModel board, int stringIndex, int fret)
    {
        return board.Strings.Value!.ElementAt(stringIndex).Frets.Value!.Single(f => f.Fret == fret);
    }

    /// <summary>
    /// Checks frets from a chord diagram written lowest string first, e.g. "x 3 2 0 1 0".
    /// The view model lists the highest string first.
    /// </summary>
    private static void Check(FretboardViewModel board, string diagram)
    {
        var frets = diagram.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < frets.Length; i++)
        {
            if (frets[i] == "x") continue;

            Fret(board, frets.Length - 1 - i, int.Parse(frets[i])).IsChecked.Value = true;
        }
    }

    [Test]
    public void Fret_IsTheFretNumber()
    {
        var fret = new FretViewModel();

        fret.Initialize(P("A", 2) + new Interval(5), new Interval(5));

        fret.Fret.Should().Be(new Interval(5));
    }

    [Test]
    public void New_HasStandardTuningStringsWithAllFrets()
    {
        var board = new FretboardViewModel();

        var strings = board.Strings.Value!.ToList();

        strings.Should().HaveCount(6);
        strings.Select(s => s.ZeroFret.Caption.Value).Should().Equal("E4", "B3", "G3", "D3", "A2", "E2");
        strings.Should().OnlyContain(s => s.Frets.Value!.Count() == 24);
    }

    [Test]
    public void New_HasNoChordName()
    {
        new FretboardViewModel().ChordNames.Value.Should().BeEmpty();
    }

    [Test]
    public void CheckingFrets_NamesTheChord()
    {
        var board = new FretboardViewModel();

        Check(board, "x 3 2 0 1 0");

        board.ChordNames.Value.Should().Be("C");
    }

    [Test]
    public void CheckingFrets_UpdatesTheNameAfterEveryChange()
    {
        var board = new FretboardViewModel();
        Check(board, "x 0 2 2 1 0");
        var names = new List<string?>();
        board.ChordNames.ListenForChange(b => names.Add(b.Value));

        Fret(board, 5, 0).IsChecked.Value = true;
        Fret(board, 5, 0).IsChecked.Value = false;

        names.Should().Equal("Am/E", "Am");
        board.ChordNames.Value.Should().Be("Am");
    }

    [Test]
    public void CheckingFrets_ListsEveryNameThatFitsSeparatedByCommas()
    {
        var board = new FretboardViewModel();

        Check(board, "x 3 2 2 1 3");

        board.ChordNames.Value.Should().Be("C6, Am7/C");
    }

    [Test]
    public void CheckingFrets_ThatFormNoChord_ClearsTheName()
    {
        var board = new FretboardViewModel();
        Check(board, "x 3 2 0 1 0");

        Fret(board, 0, 0).IsChecked.Value = false;
        Fret(board, 1, 1).IsChecked.Value = false;
        Fret(board, 2, 0).IsChecked.Value = false;
        Fret(board, 3, 2).IsChecked.Value = false;

        board.ChordNames.Value.Should().BeEmpty();
    }

    [Test]
    public void CheckingSeveralFretsOnAString_SoundsTheHighestOne()
    {
        var board = new FretboardViewModel();
        Check(board, "x 3 2 0 1 0");

        Fret(board, 4, 0).IsChecked.Value = true;

        board.ChordNames.Value.Should().Be("C");
    }

    [Test]
    public void Initialize_UsesTheGivenTuningAndFretCount()
    {
        var board = new FretboardViewModel();

        board.Initialize(new[] { P("C", 3), P("G", 3) }, 5);

        var strings = board.Strings.Value!.ToList();
        strings.Should().HaveCount(2);
        strings.Should().OnlyContain(s => s.Frets.Value!.Count() == 5);

        Fret(board, 0, 0).IsChecked.Value = true;
        Fret(board, 1, 0).IsChecked.Value = true;

        board.ChordNames.Value.Should().Be("C5");
    }

    [Test]
    public void Initialize_WithoutStrings_HasNoChordName()
    {
        var board = new FretboardViewModel();
        Check(board, "x 3 2 0 1 0");

        board.Initialize(System.Array.Empty<Pitch>());

        board.Strings.Value.Should().BeEmpty();
        board.ChordNames.Value.Should().BeEmpty();
    }

    [Test]
    public void Initialize_ClearsTheChordName()
    {
        var board = new FretboardViewModel();
        Check(board, "x 3 2 0 1 0");

        board.Initialize(Fretboard.StandardTuning);

        board.ChordNames.Value.Should().BeEmpty();
    }

    /// <summary>The sounding fret of every string as in a chord diagram, lowest string first: "x 3 2 0 1 0".</summary>
    private static string Diagram(FretboardViewModel board)
    {
        return string.Join(" ", board.Strings.Value!.Reverse().Select(s =>
        {
            var checkedFrets = s.Frets.Value!.Where(f => f.IsChecked.Value).Select(f => (int)f.Fret).ToList();
            return checkedFrets.Count == 0 ? "x" : checkedFrets.Max().ToString();
        }));
    }

    private static IEnumerable<FretViewModel> AllFrets(FretboardViewModel board)
    {
        return board.Strings.Value!.SelectMany(s => s.Frets.Value!);
    }

    private static void ShowScale(FretboardViewModel board, string root, string scale)
    {
        board.SelectedRoot.Value = root;
        board.SelectedScale.Value = board.Scales.Single(s => s.Name == scale);
        board.ShowScale.Value = true;
    }

    [Test]
    public void New_HasNoScaleShown()
    {
        var board = new FretboardViewModel();

        board.ShowScale.Value.Should().BeFalse();
        board.SelectedRoot.Value.Should().Be("C");
        board.SelectedScale.Value!.Name.Should().Be("Major");
        board.ScaleNotes.Value.Should().BeEmpty();
        board.ScaleChords.Value.Should().BeEmpty();
        AllFrets(board).Should().OnlyContain(f => !f.IsInScale.Value && !f.IsScaleRoot.Value);
    }

    [Test]
    public void Roots_AndScales_AreTheChoices()
    {
        var board = new FretboardViewModel();

        board.Roots.Should().HaveCount(17).And.Contain(new[] { "C", "C#", "Db", "Eb", "F#", "Gb", "A", "Bb", "B" });
        board.Roots.Should().OnlyHaveUniqueItems();
        board.Roots.Should().OnlyContain(r => Note.Parse(r).ToString().Length > 0);
        board.Scales.Should().BeSameAs(ScaleTemplates.All);
    }

    [Test]
    public void ShowingAScale_MarksItsNotesAndRootOnEveryString()
    {
        var board = new FretboardViewModel();

        ShowScale(board, "C", "Major");

        var inC = new[] { "C", "D", "E", "F", "G", "A", "B" }.Select(Note.Parse).ToHashSet();
        foreach (var fret in AllFrets(board))
        {
            fret.IsInScale.Value.Should().Be(inC.Contains(fret.Pitch.Note), fret.Pitch.ToString());
            fret.IsScaleRoot.Value.Should().Be(fret.Pitch.Note == Note.Parse("C"), fret.Pitch.ToString());
        }

        board.ScaleNotes.Value.Should().Be("C D E F G A B");
    }

    [Test]
    public void ShowingAScale_ListsItsChords()
    {
        var board = new FretboardViewModel();

        ShowScale(board, "C", "Major");

        board.ScaleChords.Value.Should().Be(
            "Triads: C  Dm  Em  F  G  Am  Bdim\nSevenths: Cmaj7  Dm7  Em7  Fmaj7  G7  Am7  Bm7b5");
    }

    [Test]
    public void ShowingAScale_SpellsTheScaleAndItsChordsAsTheKeyDoes()
    {
        var board = new FretboardViewModel();

        ShowScale(board, "Db", "Major");

        board.ScaleNotes.Value.Should().Be("Db Eb F Gb Ab Bb C");
        board.ScaleChords.Value.Should().StartWith("Triads: Db  Ebm  Fm  Gb  Ab  Bbm  Cdim");
    }

    [Test]
    public void ShowingAScale_NamesFretsAfterTheKey()
    {
        var board = new FretboardViewModel();
        var fSharp = AllFrets(board).First(f => f.Pitch == P("F#", 3));
        fSharp.Caption.Value.Should().Be("F#3");

        ShowScale(board, "Db", "Major");

        fSharp.Caption.Value.Should().Be("Gb3");
        // A is not in Db major, so it keeps its usual name.
        AllFrets(board).First(f => f.Pitch == P("A", 2)).Caption.Value.Should().Be("A2");
        AllFrets(board).First(f => f.Pitch == P("A", 2)).IsInScale.Value.Should().BeFalse();
    }

    [Test]
    public void ShowingAScale_MarksTheRootOnly()
    {
        var board = new FretboardViewModel();

        ShowScale(board, "G", "Major");

        var roots = AllFrets(board).Where(f => f.IsScaleRoot.Value).ToList();
        roots.Should().NotBeEmpty();
        roots.Should().OnlyContain(f => f.Pitch.Note == Note.Parse("G"));
        roots.Should().OnlyContain(f => f.IsInScale.Value);
    }

    [Test]
    public void ChangingTheRootOrScale_UpdatesTheMarks()
    {
        var board = new FretboardViewModel();
        ShowScale(board, "C", "Major");

        board.SelectedRoot.Value = "D";
        board.ScaleNotes.Value.Should().Be("D E F# G A B C#");

        board.SelectedScale.Value = board.Scales.Single(s => s.Name == "Dorian");
        board.ScaleNotes.Value.Should().Be("D E F G A B C");
        AllFrets(board).Where(f => f.Pitch.Note == Note.Parse("F#")).Should().OnlyContain(f => !f.IsInScale.Value);
    }

    [Test]
    public void ShowingAPentatonicScale_HasNoChordsToList()
    {
        var board = new FretboardViewModel();

        ShowScale(board, "C", "Major pentatonic");

        board.ScaleNotes.Value.Should().Be("C D E G A");
        board.ScaleChords.Value.Should().BeEmpty();
    }

    [Test]
    public void HidingTheScale_RemovesTheMarksAndKeyNames()
    {
        var board = new FretboardViewModel();
        ShowScale(board, "Db", "Major");

        board.ShowScale.Value = false;

        AllFrets(board).Should().OnlyContain(f => !f.IsInScale.Value && !f.IsScaleRoot.Value);
        AllFrets(board).First(f => f.Pitch == P("F#", 3)).Caption.Value.Should().Be("F#3");
        board.ScaleNotes.Value.Should().BeEmpty();
        board.ScaleChords.Value.Should().BeEmpty();
    }

    [Test]
    public void Initialize_KeepsShowingTheScale()
    {
        var board = new FretboardViewModel();
        ShowScale(board, "C", "Major");

        board.Initialize(new[] { P("E", 4), P("B", 3) }, 6);

        AllFrets(board).Should().HaveCount(12);
        AllFrets(board).Where(f => f.IsInScale.Value).Select(f => f.Pitch.Note.ToString())
            .Should().OnlyContain(n => new[] { "C", "D", "E", "F", "G", "A", "B" }.Contains(n));
        AllFrets(board).Any(f => f.IsInScale.Value).Should().BeTrue();
        AllFrets(board).First(f => f.Pitch == P("C", 4)).IsScaleRoot.Value.Should().BeTrue();
    }

    [Test]
    public void ChordNames_AreSpelledAsTheShownScaleSpellsThem()
    {
        var board = new FretboardViewModel();
        Check(board, "2 4 4 3 2 2");
        board.ChordNames.Value.Should().Be("F#");

        ShowScale(board, "Db", "Major");
        board.ChordNames.Value.Should().Be("Gb");

        board.ShowScale.Value = false;
        board.ChordNames.Value.Should().Be("F#");
    }

    [Test]
    public void Transpose_MovesEveryCheckedFret()
    {
        var board = new FretboardViewModel();
        Check(board, "x 3 2 0 1 0");

        board.TransposeUp.Execute(null);

        Diagram(board).Should().Be("x 4 3 1 2 1");
        board.ChordNames.Value.Should().Be("Db");

        board.TransposeDown.Execute(null);
        Diagram(board).Should().Be("x 3 2 0 1 0");

        // The open strings cannot go any lower, so nothing moves.
        board.TransposeDown.CanExecute(null).Should().BeFalse();
        board.TransposeDown.Execute(null);
        Diagram(board).Should().Be("x 3 2 0 1 0");
    }

    [Test]
    public void Transpose_KeepsAllFretsCheckedOnAString()
    {
        var board = new FretboardViewModel();
        Fret(board, 0, 3).IsChecked.Value = true;
        Fret(board, 0, 5).IsChecked.Value = true;

        board.TransposeUp.Execute(null);

        AllFrets(board).Where(f => f.IsChecked.Value).Select(f => (int)f.Fret).Should().BeEquivalentTo(new[] { 4, 6 });
    }

    [Test]
    public void Transpose_IsOnlyPossibleWhileFretsAreChecked()
    {
        var board = new FretboardViewModel();

        board.TransposeUp.CanExecute(null).Should().BeFalse();
        board.TransposeDown.CanExecute(null).Should().BeFalse();

        Fret(board, 0, 5).IsChecked.Value = true;

        board.TransposeUp.CanExecute(null).Should().BeTrue();
        board.TransposeDown.CanExecute(null).Should().BeTrue();

        Fret(board, 0, 5).IsChecked.Value = false;

        board.TransposeUp.CanExecute(null).Should().BeFalse();
        board.TransposeDown.CanExecute(null).Should().BeFalse();
    }

    [Test]
    public void Transpose_StopsAtTheEndsOfTheNeck()
    {
        var board = new FretboardViewModel();
        Fret(board, 0, 0).IsChecked.Value = true;

        board.TransposeDown.CanExecute(null).Should().BeFalse();
        board.TransposeUp.CanExecute(null).Should().BeTrue();
        board.TransposeDown.Execute(null);
        Diagram(board).Should().Be("x x x x x 0");

        Fret(board, 0, 0).IsChecked.Value = false;
        Fret(board, 0, 23).IsChecked.Value = true;

        board.TransposeUp.CanExecute(null).Should().BeFalse();
        board.TransposeDown.CanExecute(null).Should().BeTrue();
        board.TransposeUp.Execute(null);
        Diagram(board).Should().Be("x x x x x 23");
    }

    [Test]
    public void Transpose_RaisesCanExecuteChanged()
    {
        var board = new FretboardViewModel();
        var raised = 0;
        board.TransposeUp.CanExecuteChanged += (_, _) => raised++;

        Fret(board, 0, 5).IsChecked.Value = true;

        raised.Should().BeGreaterThan(0);
    }

    [Test]
    public void Transpose_UpdatesTheChordNameOnce()
    {
        var board = new FretboardViewModel();
        Check(board, "0 2 2 1 0 0");
        var names = new List<string?>();
        board.ChordNames.ListenForChange(b => names.Add(b.Value));

        board.TransposeUp.Execute(null);
        board.TransposeUp.Execute(null);

        names.Should().Equal("F", "F#");
    }

    [Test]
    public void Clear_UnchecksEveryFret()
    {
        var board = new FretboardViewModel();
        Check(board, "x 3 2 0 1 0");

        board.ClearFrets.Execute(null);

        AllFrets(board).Should().OnlyContain(f => !f.IsChecked.Value);
        board.ChordNames.Value.Should().BeEmpty();
        board.TransposeUp.CanExecute(null).Should().BeFalse();
    }

    [Test]
    public void ChordQuery_FindsShapes()
    {
        var board = new FretboardViewModel();

        board.ChordQuery.Value = "Am";

        board.Shapes.Value.Should().Contain("x 0 2 2 1 0");
        board.Shapes.Value.Should().HaveCountLessOrEqualTo(12);
        board.ChordQueryStatus.Value.Should().Be($"{board.Shapes.Value!.Count()} shapes");
    }

    [Test]
    public void ChordQuery_WithSlashChord_FindsShapesWithThatBass()
    {
        var board = new FretboardViewModel();

        board.ChordQuery.Value = "C/G";

        board.Shapes.Value.Should().Contain("3 3 2 0 1 0");
    }

    [Test]
    public void ChordQuery_WithNothingTyped_HasNoShapesOrStatus()
    {
        var board = new FretboardViewModel();
        board.ChordQuery.Value = "Am";

        board.ChordQuery.Value = "";

        board.Shapes.Value.Should().BeEmpty();
        board.ChordQueryStatus.Value.Should().BeEmpty();
    }

    [Test]
    public void ChordQuery_WithSomethingThatIsNotAChord_SaysSo()
    {
        var board = new FretboardViewModel();

        board.ChordQuery.Value = "Xyz";

        board.Shapes.Value.Should().BeEmpty();
        board.ChordQueryStatus.Value.Should().Be("Not a chord");
    }

    [Test]
    public void ChordQuery_WithAChordNobodyCanPlay_SaysNoShapes()
    {
        var board = new FretboardViewModel();
        board.Initialize(new[] { P("E", 4), P("B", 3) });

        board.ChordQuery.Value = "C";

        board.Shapes.Value.Should().BeEmpty();
        board.ChordQueryStatus.Value.Should().Be("No shapes found");
    }

    [Test]
    public void ChordQuery_IgnoresSurroundingSpaces()
    {
        var board = new FretboardViewModel();

        board.ChordQuery.Value = "  Am ";

        board.Shapes.Value.Should().Contain("x 0 2 2 1 0");
    }

    [Test]
    public void ChoosingAShape_ChecksItsFrets()
    {
        var board = new FretboardViewModel();
        board.ChordQuery.Value = "Am";

        board.SelectedShape.Value = "x 0 2 2 1 0";

        Diagram(board).Should().Be("x 0 2 2 1 0");
        board.ChordNames.Value.Should().Be("Am");
    }

    [Test]
    public void ChoosingAnotherShape_ReplacesTheFrets()
    {
        var board = new FretboardViewModel();
        board.ChordQuery.Value = "C";
        Check(board, "x x x x x 3");

        board.SelectedShape.Value = "x 3 2 0 1 0";
        Diagram(board).Should().Be("x 3 2 0 1 0");

        board.SelectedShape.Value = board.Shapes.Value!.First(s => s != "x 3 2 0 1 0");
        Diagram(board).Should().NotBe("x 3 2 0 1 0");
        board.ChordNames.Value.Should().StartWith("C");
        AllFrets(board).Count(f => f.IsChecked.Value).Should().BeLessThan(7);
    }

    [Test]
    public void ChoosingAShape_MakesTransposingPossible()
    {
        var board = new FretboardViewModel();
        board.ChordQuery.Value = "G";

        board.SelectedShape.Value = "3 2 0 0 0 3";
        board.TransposeUp.Execute(null);

        board.ChordNames.Value.Should().StartWith("Ab");
    }

    [Test]
    public void ChangingTheQuery_ClearsTheChosenShape()
    {
        var board = new FretboardViewModel();
        board.ChordQuery.Value = "Am";
        board.SelectedShape.Value = "x 0 2 2 1 0";

        board.ChordQuery.Value = "Em";

        board.SelectedShape.Value.Should().BeNull();
        board.Shapes.Value.Should().Contain("0 2 2 0 0 0");
    }

    [Test]
    public void ChoosingNothing_LeavesTheFretsAlone()
    {
        var board = new FretboardViewModel();
        board.ChordQuery.Value = "Am";
        board.SelectedShape.Value = "x 0 2 2 1 0";

        board.SelectedShape.Value = null;

        Diagram(board).Should().Be("x 0 2 2 1 0");
    }

    [Test]
    public void ChoosingAShape_ThatIsNotInTheList_DoesNothing()
    {
        var board = new FretboardViewModel();
        board.ChordQuery.Value = "Am";

        board.SelectedShape.Value = "x x x x x x";

        Diagram(board).Should().Be("x x x x x x");
    }

    [Test]
    public void ChoosingAShape_UsesTheKeyToSpellTheChord()
    {
        var board = new FretboardViewModel();
        ShowScale(board, "Db", "Major");
        board.ChordQuery.Value = "F#";

        board.SelectedShape.Value = "2 4 4 3 2 2";

        board.ChordNames.Value.Should().Be("Gb");
    }

    [Test]
    public void New_UsesStandardTuning()
    {
        var board = new FretboardViewModel();

        board.Tunings.Should().BeSameAs(TuningTemplates.All);
        board.SelectedTuning.Value.Should().BeSameAs(TuningTemplates.Standard);
    }

    [Test]
    public void ChoosingATuning_ReplacesTheStrings()
    {
        var board = new FretboardViewModel();

        board.SelectedTuning.Value = TuningTemplates.DropD;

        board.Strings.Value!.Select(s => s.ZeroFret.Caption.Value).Should().Equal("E4", "B3", "G3", "D3", "A2", "D2");
        board.Strings.Value!.Should().OnlyContain(s => s.Frets.Value!.Count() == 24);
    }

    [Test]
    public void ChoosingATuning_WithAnotherNumberOfStrings_ChangesTheNumberOfStrings()
    {
        var board = new FretboardViewModel();

        board.SelectedTuning.Value = TuningTemplates.Bass;
        board.Strings.Value!.Should().HaveCount(4);

        board.SelectedTuning.Value = TuningTemplates.Standard;
        board.Strings.Value!.Should().HaveCount(6);
    }

    [Test]
    public void ChoosingATuning_UnchecksTheFrets()
    {
        var board = new FretboardViewModel();
        Check(board, "x 3 2 0 1 0");

        board.SelectedTuning.Value = TuningTemplates.DropD;

        AllFrets(board).Should().OnlyContain(f => !f.IsChecked.Value);
        board.ChordNames.Value.Should().BeEmpty();
        board.TransposeUp.CanExecute(null).Should().BeFalse();
    }

    [Test]
    public void ChoosingATuning_NamesChordsAfterTheNewStrings()
    {
        var board = new FretboardViewModel();
        board.SelectedTuning.Value = TuningTemplates.OpenG;

        foreach (var (stringIndex, _) in board.Strings.Value!.Select((s, i) => (i, s)))
            Fret(board, stringIndex, 0).IsChecked.Value = true;

        board.ChordNames.Value.Should().StartWith("G");
    }

    [Test]
    public void ChoosingATuning_FindsShapesForTheNewStrings()
    {
        var board = new FretboardViewModel();
        board.ChordQuery.Value = "D";
        var standardShapes = board.Shapes.Value!.ToList();

        board.SelectedTuning.Value = TuningTemplates.OpenD;

        board.Shapes.Value.Should().Contain("0 0 0 0 0 0");
        board.Shapes.Value.Should().NotEqual(standardShapes);
        board.SelectedShape.Value.Should().BeNull();
    }

    [Test]
    public void ChoosingATuning_KeepsShowingTheScale()
    {
        var board = new FretboardViewModel();
        ShowScale(board, "C", "Major");

        board.SelectedTuning.Value = TuningTemplates.Bass;

        AllFrets(board).Should().HaveCount(4 * 24);
        AllFrets(board).Where(f => f.IsInScale.Value).Should().NotBeEmpty();
        AllFrets(board).Where(f => f.IsScaleRoot.Value).Should().OnlyContain(f => f.Pitch.Note == Note.Parse("C"));
        board.ScaleNotes.Value.Should().Be("C D E F G A B");
    }

    [Test]
    public void ChoosingATuning_ThenShape_ChecksFretsOfTheNewStrings()
    {
        var board = new FretboardViewModel();
        board.SelectedTuning.Value = TuningTemplates.Ukulele;
        board.ChordQuery.Value = "C";

        board.Shapes.Value.Should().NotBeEmpty();
        board.SelectedShape.Value = board.Shapes.Value!.First();

        board.ChordNames.Value.Should().StartWith("C");
        Diagram(board).Split(' ').Should().HaveCount(4);
    }

    [Test]
    public void New_HasNoCapo()
    {
        var board = new FretboardViewModel();

        board.SelectedCapo.Value.Should().Be(0);
        board.Capos.Should().Equal(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
    }

    [Test]
    public void Capo_RaisesEveryStringAndShortensTheNeck()
    {
        var board = new FretboardViewModel();

        board.SelectedCapo.Value = 2;

        board.Strings.Value!.Select(s => s.ZeroFret.Caption.Value).Should().Equal("F#4", "C#4", "A3", "E3", "B2", "F#2");
        board.Strings.Value!.Should().OnlyContain(s => s.Frets.Value!.Count() == 22);
    }

    [Test]
    public void Capo_CountsFretsFromTheCapo()
    {
        var board = new FretboardViewModel();
        board.SelectedCapo.Value = 3;

        // The fifth fret from the capo on the low string is the eighth fret: E2 + 8 = C3.
        Fret(board, 5, 5).Pitch.Should().Be(P("C", 3));
        Fret(board, 5, 0).Pitch.Should().Be(P("G", 2));
    }

    [Test]
    public void Capo_NamesChordsByTheirRealNotes()
    {
        var board = new FretboardViewModel();
        board.SelectedCapo.Value = 2;

        // The open E shape with a capo on the second fret is F#.
        Check(board, "0 2 2 1 0 0");

        board.ChordNames.Value.Should().Be("F#");
    }

    [Test]
    public void Capo_FindsShapesRelativeToTheCapo()
    {
        var board = new FretboardViewModel();
        board.SelectedCapo.Value = 2;

        board.ChordQuery.Value = "E";

        // With the capo on fret 2, the D shape sounds E.
        board.Shapes.Value.Should().Contain("x x 0 2 3 2");
        board.ChordQueryStatus.Value.Should().EndWith("(counted from the capo)");

        board.SelectedShape.Value = "x x 0 2 3 2";
        board.ChordNames.Value.Should().Be("E");
    }

    [Test]
    public void Capo_IsNotMentionedWithoutOne()
    {
        var board = new FretboardViewModel();

        board.ChordQuery.Value = "E";

        board.ChordQueryStatus.Value.Should().NotContain("capo");
    }

    [Test]
    public void Capo_UnchecksTheFretsAndKeepsTheTuning()
    {
        var board = new FretboardViewModel();
        board.SelectedTuning.Value = TuningTemplates.DropD;
        Check(board, "x 3 2 0 1 0");

        board.SelectedCapo.Value = 1;

        AllFrets(board).Should().OnlyContain(f => !f.IsChecked.Value);
        board.SelectedTuning.Value.Should().BeSameAs(TuningTemplates.DropD);
        board.Strings.Value!.Select(s => s.ZeroFret.Caption.Value).Should().Equal("F4", "C4", "G#3", "D#3", "A#2", "D#2");
    }

    [Test]
    public void Tuning_KeepsTheCapo()
    {
        var board = new FretboardViewModel();
        board.SelectedCapo.Value = 2;

        board.SelectedTuning.Value = TuningTemplates.Bass;

        board.SelectedCapo.Value.Should().Be(2);
        board.Strings.Value!.Select(s => s.ZeroFret.Caption.Value).Should().Equal("A2", "E2", "B1", "F#1");
        board.Strings.Value!.Should().OnlyContain(s => s.Frets.Value!.Count() == 22);
    }

    [Test]
    public void Capo_BackToNone_RestoresTheNeck()
    {
        var board = new FretboardViewModel();
        board.SelectedCapo.Value = 5;

        board.SelectedCapo.Value = 0;

        board.Strings.Value!.Select(s => s.ZeroFret.Caption.Value).Should().Equal("E4", "B3", "G3", "D3", "A2", "E2");
        board.Strings.Value!.Should().OnlyContain(s => s.Frets.Value!.Count() == 24);
    }

    [Test]
    public void Capo_KeepsShowingTheScale()
    {
        var board = new FretboardViewModel();
        ShowScale(board, "C", "Major");

        board.SelectedCapo.Value = 2;

        // F#4 (the open high string with the capo) is not in C major, but G4 (the next fret) is.
        Fret(board, 0, 0).IsInScale.Value.Should().BeFalse();
        Fret(board, 0, 1).IsInScale.Value.Should().BeTrue();
    }
}
