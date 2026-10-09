using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class FretboardTests
{
    private static Pitch P(string note, int octave)
    {
        return new Pitch(new Octave(octave), Note.Parse(note));
    }

    /// <summary>
    /// Builds a standard-tuned fretboard from a chord diagram written the usual way:
    /// frets from the lowest string to the highest, "x" for a muted string, e.g. "x 3 2 0 1 0".
    /// </summary>
    private static Fretboard Shape(string diagram)
    {
        var frets = diagram.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        frets.Should().HaveCount(6);

        var board = Fretboard.Standard();

        for (var i = 0; i < frets.Length; i++)
        {
            if (frets[i] == "x") continue;

            // The fretboard lists the highest string first, diagrams list the lowest first.
            board = board.PressFret(frets.Length - 1 - i, int.Parse(frets[i]));
        }

        return board;
    }

    private static string[] Names(Fretboard board)
    {
        return board.GetChordNames().Select(c => c.ToString()).ToArray();
    }

    [Test]
    public void Constructor_SetsStringsAndFrets()
    {
        var tunings = new[] { P("E", 4), P("B", 3) };

        var board = new Fretboard(tunings, 12);

        board.StringCount.Should().Be(2);
        board.FretCount.Should().Be(12);
        board.Tunings.Should().Equal(tunings);
    }

    [Test]
    public void Constructor_WithoutFretCount_UsesDefault()
    {
        new Fretboard(new[] { P("E", 4) }).FretCount.Should().Be(Fretboard.DefaultFretCount);
    }

    [Test]
    public void Constructor_WithoutStrings_Throws()
    {
        var act = () => new Fretboard(Array.Empty<Pitch>());

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_WithNoFrets_Throws(int fretCount)
    {
        var act = () => new Fretboard(new[] { P("E", 4) }, fretCount);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Standard_IsStandardGuitarTuningFromHighestString()
    {
        var board = Fretboard.Standard();

        board.StringCount.Should().Be(6);
        board.FretCount.Should().Be(Fretboard.DefaultFretCount);
        board.Tunings.Select(t => t.ToString()).Should().Equal("E4", "B3", "G3", "D3", "A2", "E2");
    }

    [Test]
    public void Standard_WithFretCount_UsesIt()
    {
        Fretboard.Standard(12).FretCount.Should().Be(12);
    }

    [Test]
    public void New_HasNothingPressedAndNoSoundingPitches()
    {
        var board = Fretboard.Standard();

        for (var s = 0; s < board.StringCount; s++)
        {
            board.IsPressed(s, 0).Should().BeFalse();
            board.GetPitch(s).Should().BeNull();
        }

        board.GetPitches().Should().BeEmpty();
        board.GetChordNames().Should().BeEmpty();
    }

    [Test]
    public void PressFret_PressesOnlyThatFretOnThatString()
    {
        var board = Fretboard.Standard().PressFret(2, 5);

        board.IsPressed(2, 5).Should().BeTrue();
        board.IsPressed(2, 4).Should().BeFalse();
        board.IsPressed(1, 5).Should().BeFalse();
        board.IsPressed(3, 5).Should().BeFalse();
    }

    [Test]
    public void PressFret_CanPressTheOpenStringAndTheLastFret()
    {
        var board = Fretboard.Standard(12).PressFret(0, 0).PressFret(0, 11);

        board.IsPressed(0, 0).Should().BeTrue();
        board.IsPressed(0, 11).Should().BeTrue();
    }

    [Test]
    public void PressFret_DoesNotChangeTheOriginal()
    {
        var original = Fretboard.Standard();

        var pressed = original.PressFret(0, 3);

        pressed.Should().NotBeSameAs(original);
        original.IsPressed(0, 3).Should().BeFalse();
    }

    [Test]
    public void PressFret_WhenAlreadyPressed_ReturnsSameBoard()
    {
        var board = Fretboard.Standard().PressFret(0, 3);

        board.PressFret(0, 3).Should().BeSameAs(board);
    }

    [Test]
    public void PressFret_CanPressSeveralFretsOnOneString()
    {
        var board = Fretboard.Standard().PressFret(1, 0).PressFret(1, 3);

        board.IsPressed(1, 0).Should().BeTrue();
        board.IsPressed(1, 3).Should().BeTrue();
    }

    [Test]
    [TestCase(-1)]
    [TestCase(6)]
    public void PressFret_OnMissingString_Throws(int stringIndex)
    {
        var act = () => Fretboard.Standard().PressFret(stringIndex, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    [TestCase(-1)]
    [TestCase(24)]
    [TestCase(100)]
    public void PressFret_OnMissingFret_Throws(int fret)
    {
        var act = () => Fretboard.Standard().PressFret(0, fret);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void DepressFret_RemovesOnlyThatFret()
    {
        var board = Fretboard.Standard()
            .PressFret(0, 1)
            .PressFret(0, 2)
            .PressFret(1, 1)
            .DepressFret(0, 1);

        board.IsPressed(0, 1).Should().BeFalse();
        board.IsPressed(0, 2).Should().BeTrue();
        board.IsPressed(1, 1).Should().BeTrue();
    }

    [Test]
    public void DepressFret_DoesNotChangeTheOriginal()
    {
        var original = Fretboard.Standard().PressFret(0, 1);

        var depressed = original.DepressFret(0, 1);

        depressed.Should().NotBeSameAs(original);
        original.IsPressed(0, 1).Should().BeTrue();
    }

    [Test]
    public void DepressFret_WhenNotPressed_ReturnsSameBoard()
    {
        var board = Fretboard.Standard().PressFret(0, 1);

        board.DepressFret(0, 2).Should().BeSameAs(board);
        board.DepressFret(1, 1).Should().BeSameAs(board);
    }

    [Test]
    [TestCase(-1)]
    [TestCase(6)]
    public void DepressFret_OnMissingString_Throws(int stringIndex)
    {
        var act = () => Fretboard.Standard().DepressFret(stringIndex, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    [TestCase(-1)]
    [TestCase(6)]
    public void IsPressed_OnMissingString_Throws(int stringIndex)
    {
        var act = () => Fretboard.Standard().IsPressed(stringIndex, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void IsPressed_OnMissingFret_IsFalse()
    {
        var board = Fretboard.Standard();

        board.IsPressed(0, -1).Should().BeFalse();
        board.IsPressed(0, 24).Should().BeFalse();
    }

    [Test]
    public void GetPitch_OfOpenString_IsTheTuning()
    {
        var board = Fretboard.Standard().PressFret(4, 0);

        board.GetPitch(4).Should().Be(P("A", 2));
    }

    [Test]
    [TestCase(3, "C", 3)]
    [TestCase(12, "A", 3)]
    [TestCase(15, "C", 4)]
    public void GetPitch_OfPressedFret_IsTheTuningTransposedByTheFret(int fret, string note, int octave)
    {
        var board = Fretboard.Standard().PressFret(4, fret);

        board.GetPitch(4).Should().Be(P(note, octave));
    }

    [Test]
    public void GetPitch_WithSeveralFretsPressed_SoundsTheHighestOne()
    {
        var board = Fretboard.Standard().PressFret(4, 0).PressFret(4, 7).PressFret(4, 3);

        board.GetPitch(4).Should().Be(P("E", 3));
    }

    [Test]
    public void GetPitch_OfMutedString_IsNull()
    {
        Fretboard.Standard().PressFret(0, 1).GetPitch(1).Should().BeNull();
    }

    [Test]
    public void GetPitch_OnMissingString_Throws()
    {
        var act = () => Fretboard.Standard().GetPitch(6);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetPitches_ReturnsSoundingPitchesInStringOrder_SkippingMutedStrings()
    {
        var board = Shape("x 3 2 0 1 0");

        board.GetPitches().Select(p => p.ToString())
            .Should().Equal("E4", "C4", "G3", "E3", "C3");
    }

    [Test]
    public void Transpose_MovesEveryPressedFret()
    {
        var board = Fretboard.Standard().PressFret(0, 1).PressFret(0, 4).PressFret(3, 2);

        var transposed = board.Transpose(3);

        transposed.IsPressed(0, 4).Should().BeTrue();
        transposed.IsPressed(0, 7).Should().BeTrue();
        transposed.IsPressed(3, 5).Should().BeTrue();
        transposed.IsPressed(0, 1).Should().BeFalse();
        transposed.IsPressed(3, 2).Should().BeFalse();
    }

    [Test]
    public void Transpose_ByNegativeInterval_MovesTowardsTheNut()
    {
        var board = Fretboard.Standard().PressFret(1, 5).Transpose(-2);

        board.IsPressed(1, 3).Should().BeTrue();
        board.IsPressed(1, 5).Should().BeFalse();
    }

    [Test]
    public void Transpose_ByZero_KeepsTheSameFrets()
    {
        var board = Shape("x 3 2 0 1 0").Transpose(0);

        Names(board).Should().Equal("C");
    }

    [Test]
    public void Transpose_DoesNotChangeTheOriginal()
    {
        var original = Fretboard.Standard().PressFret(0, 1);

        original.Transpose(2);

        original.IsPressed(0, 1).Should().BeTrue();
        original.IsPressed(0, 3).Should().BeFalse();
    }

    [Test]
    public void Transpose_PastTheLastFret_Throws()
    {
        var board = Fretboard.Standard().PressFret(0, 19);

        board.Invoking(b => b.Transpose(4)).Should().NotThrow();
        board.Invoking(b => b.Transpose(5)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Transpose_PastTheNut_Throws()
    {
        var board = Fretboard.Standard().PressFret(0, 2);

        board.Invoking(b => b.Transpose(-2)).Should().NotThrow();
        board.Invoking(b => b.Transpose(-3)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Transpose_WithNothingPressed_Succeeds()
    {
        Fretboard.Standard().Transpose(30).GetPitches().Should().BeEmpty();
    }

    [Test]
    public void Transpose_RaisesTheSoundingPitchesByTheInterval()
    {
        var board = Shape("x 3 2 0 1 0");

        var before = board.GetPitches().ToList();
        var after = board.Transpose(2).GetPitches().ToList();

        after.Should().HaveCount(before.Count);
        after.Zip(before, (a, b) => a - b).Should().OnlyContain(i => i == new Interval(2));
    }

    [Test]
    [TestCase("x 3 2 0 1 0", "C")]
    [TestCase("x 0 2 2 1 0", "Am")]
    [TestCase("3 2 0 0 0 3", "G")]
    [TestCase("0 2 2 1 0 0", "E")]
    [TestCase("0 2 2 0 0 0", "Em")]
    [TestCase("x x 0 2 3 2", "D")]
    [TestCase("x x 0 2 3 1", "Dm")]
    [TestCase("x 0 2 2 2 0", "A")]
    [TestCase("x x 3 2 1 1", "F")]
    [TestCase("1 3 3 2 1 1", "F")]
    [TestCase("x 2 4 4 3 2", "Bm")]
    [TestCase("3 2 0 0 0 1", "G7")]
    [TestCase("0 2 0 1 0 0", "E7")]
    [TestCase("x 0 2 0 2 0", "A7")]
    [TestCase("x 3 2 0 0 0", "Cmaj7")]
    [TestCase("x 0 2 0 1 0", "Am7")]
    [TestCase("x 0 2 2 1 3", "Am7")]
    [TestCase("x x 0 2 1 1", "Dm7")]
    [TestCase("x x 0 2 3 0", "Dsus2")]
    [TestCase("x 0 2 2 3 0", "Asus4")]
    [TestCase("x x 0 2 3 3", "Dsus4")]
    [TestCase("x 3 2 0 3 3", "Cadd9")]
    [TestCase("0 2 2 x x x", "E5")]
    [TestCase("x 0 2 2 x x", "A5")]
    [TestCase("0 3 2 0 1 0", "C/E")]
    [TestCase("3 3 2 0 1 0", "C/G")]
    [TestCase("x x 2 2 1 0", "Am/E")]
    public void GetChordNames_NamesCommonChordShapes(string diagram, string expected)
    {
        Names(Shape(diagram)).First().Should().Be(expected);
    }

    [Test]
    public void GetChordNames_ListsEveryNameThatFits()
    {
        Names(Shape("x x 0 2 3 3")).Should().Equal("Dsus4", "Gsus2/D");
        Names(Shape("x 0 2 0 1 0")).Should().Equal("Am7", "C6/A");
        Names(Shape("x x 1 2 1 2")).Should().Equal("D#dim7", "Cdim7/D#", "F#dim7/D#", "Adim7/D#");
    }

    [Test]
    [TestCase("x x x x x x")]
    [TestCase("x x x x x 0")]
    [TestCase("0 x x x x x")]
    [TestCase("x x x x 0 1")]
    public void GetChordNames_WithoutAChord_IsEmpty(string diagram)
    {
        Names(Shape(diagram)).Should().BeEmpty();
    }

    [Test]
    public void GetChordNames_UsesTheLowestSoundingStringAsTheBass()
    {
        var am = Shape("x 0 2 2 1 0");
        Names(am).Should().Equal("Am");

        var withLowE = am.PressFret(5, 0);
        Names(withLowE).Should().Equal("Am/E");

        Names(withLowE.DepressFret(5, 0)).Should().Equal("Am");
    }

    [Test]
    public void GetChordNames_WithSeveralFretsOnAString_UsesTheHighestOne()
    {
        // With the open A string sounding this would be an Am7-type chord, not C.
        var board = Shape("x 3 2 0 1 0").PressFret(4, 0);

        Names(board).Should().Equal("C");
    }

    [Test]
    public void GetChordNames_AfterTransposingAShape_NamesTheNewChord()
    {
        Names(Shape("0 2 2 1 0 0").Transpose(5)).Should().Equal("A");
        Names(Shape("1 3 3 2 1 1").Transpose(2)).Should().Equal("G");
        Names(Shape("x 0 2 2 1 0").Transpose(2)).Should().Equal("Bm");
    }

    [Test]
    public void GetChordNames_UsesTheBoardTuning()
    {
        // Drop D: the lowest string is D2 instead of E2.
        var dropD = new Fretboard(new[] { P("E", 4), P("B", 3), P("G", 3), P("D", 3), P("A", 2), P("D", 2) });

        var board = dropD.PressFret(5, 0).PressFret(4, 0).PressFret(3, 0).PressFret(2, 2);

        // D A D A, with the G string pressed on fret 2 (A): a D power chord.
        Names(board).Should().Equal("D5");
    }
}
