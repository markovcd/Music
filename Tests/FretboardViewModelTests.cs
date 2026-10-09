using System.Collections.Generic;
using System.Linq;
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
}
