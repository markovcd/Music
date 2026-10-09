using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class FingeringTests
{
    /// <summary>A shape written as in a chord diagram (lowest string first, "x" for muted), analysed on a standard board.</summary>
    private static Fingering Analyze(string diagram)
    {
        var frets = diagram.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f == "x" ? (int?)null : int.Parse(f))
            .Reverse()
            .ToList();

        return Fingering.Analyze(frets);
    }

    [Test]
    public void Analyze_OpenC_NeedsThreeFingers()
    {
        var fingering = Analyze("x 3 2 0 1 0");

        fingering.Fingers.Should().Be(3);
        fingering.PressedNotes.Should().Be(3);
        fingering.BarreFret.Should().BeNull();
        fingering.Span.Should().Be(3);
        fingering.HighestFret.Should().Be(3);
        fingering.MutedStrings.Should().Be(1);
    }

    [Test]
    public void Analyze_NeighbouringStringsAtTheSameFret_ShareAFinger()
    {
        // E major: the two notes at fret 2 can be played with one finger lying flat.
        var fingering = Analyze("0 2 2 1 0 0");

        fingering.Fingers.Should().Be(2);
        fingering.PressedNotes.Should().Be(3);
        fingering.BarreFret.Should().BeNull();
        fingering.Span.Should().Be(2);
        fingering.HighestFret.Should().Be(2);
        fingering.MutedStrings.Should().Be(0);
    }

    [Test]
    public void Analyze_StringsAtTheSameFretWithAGapBetween_NeedTheirOwnFinger()
    {
        // G major: the notes at fret 3 are on the lowest and highest string with open strings between them.
        var fingering = Analyze("3 2 0 0 0 3");

        fingering.Fingers.Should().Be(3);
        fingering.PressedNotes.Should().Be(3);
        fingering.BarreFret.Should().BeNull();
    }

    [Test]
    public void Analyze_TheLowestFretOnSeveralStrings_IsABarre()
    {
        var fingering = Analyze("1 3 3 2 1 1");

        fingering.BarreFret.Should().Be(1);
        fingering.Fingers.Should().Be(3);
        fingering.PressedNotes.Should().Be(3);
        fingering.Span.Should().Be(3);
        fingering.HighestFret.Should().Be(3);
        fingering.MutedStrings.Should().Be(0);
    }

    [Test]
    public void Analyze_ABarreMayStartAboveTheNut_AndLeaveLowStringsMuted()
    {
        var fingering = Analyze("x 2 4 4 3 2");

        fingering.BarreFret.Should().Be(2);
        fingering.Fingers.Should().Be(3);
        fingering.PressedNotes.Should().Be(3);
        fingering.Span.Should().Be(3);
        fingering.HighestFret.Should().Be(4);
        fingering.MutedStrings.Should().Be(1);
    }

    [Test]
    public void Analyze_AnOpenStringInsideTheBarre_MeansNoBarre()
    {
        // Same frets as an F barre chord, but the A string is open: the first finger cannot lie across.
        var fingering = Analyze("1 0 3 2 1 1");

        fingering.BarreFret.Should().BeNull();
        fingering.Fingers.Should().Be(4);
        fingering.PressedNotes.Should().Be(5);
    }

    [Test]
    public void Analyze_AMutedStringInsideTheBarre_MeansNoBarre()
    {
        var fingering = Analyze("3 x 5 5 3 3");

        fingering.BarreFret.Should().BeNull();
        fingering.PressedNotes.Should().Be(5);
    }

    [Test]
    public void Analyze_OnlyTheLowestFretCanBeABarre()
    {
        // The notes at fret 5 are next to each other (one flat finger), but that is not a barre.
        var fingering = Analyze("x 3 5 5 5 x");

        fingering.BarreFret.Should().BeNull();
        fingering.Fingers.Should().Be(2);
        fingering.PressedNotes.Should().Be(4);
    }

    [Test]
    public void Analyze_ShapeWithoutFrettedNotes_NeedsNoFingers()
    {
        var fingering = Analyze("0 0 0 0 0 0");

        fingering.Fingers.Should().Be(0);
        fingering.PressedNotes.Should().Be(0);
        fingering.BarreFret.Should().BeNull();
        fingering.Span.Should().Be(0);
        fingering.HighestFret.Should().Be(0);
        fingering.MutedStrings.Should().Be(0);
        fingering.Difficulty.Should().Be(0);
    }

    [Test]
    public void Analyze_ShapeWithNothingSounding_HasOnlyMutedStrings()
    {
        var fingering = Analyze("x x x x x x");

        fingering.Fingers.Should().Be(0);
        fingering.MutedStrings.Should().Be(6);
    }

    [Test]
    public void Analyze_SingleNote_NeedsOneFinger()
    {
        var fingering = Analyze("x x x x x 7");

        fingering.Fingers.Should().Be(1);
        fingering.Span.Should().Be(1);
        fingering.HighestFret.Should().Be(7);
    }

    [Test]
    public void Analyze_Board_UsesTheSoundingFretOfEachString()
    {
        // The highest pressed fret sounds, so pressing fret 0 as well as 3 is the same as fret 3.
        var board = Fretboard.Standard().PressFret(4, 0).PressFret(4, 3).PressFret(3, 2).PressFret(2, 0).PressFret(1, 1).PressFret(0, 0);

        Fingering.Analyze(board).Should().Be(Analyze("x 3 2 0 1 0"));
        Fingering.Analyze(board).Should().Be(Analyze(board.Diagram));
    }

    [Test]
    public void Analyze_EmptyBoard_HasOnlyMutedStrings()
    {
        Fingering.Analyze(Fretboard.Standard()).MutedStrings.Should().Be(6);
    }

    [Test]
    [TestCase("x 3 2 0 1 0", 22)]
    [TestCase("1 3 3 2 1 1", 23)]
    [TestCase("0 2 2 1 0 0", 15)]
    [TestCase("3 2 0 0 0 3", 16)]
    [TestCase("x x x x x 0", 20)]
    public void Difficulty_AddsUpTheEffortOfTheShape(string diagram, int difficulty)
    {
        // 3 for each pressed note, 5 for a barre, 2 for each fret of stretch, 1 for each fret up the neck, 4 for each muted string.
        Analyze(diagram).Difficulty.Should().Be(difficulty);
    }

    [Test]
    public void Difficulty_IsLowerForAFullChordThanForPartOfIt()
    {
        Analyze("x 3 2 0 1 0").Difficulty.Should().BeLessThan(Analyze("x 3 2 0 x x").Difficulty);
        Analyze("3 2 0 0 0 3").Difficulty.Should().BeLessThan(Analyze("3 2 0 x x x").Difficulty);
    }

    [Test]
    public void Difficulty_IsLowerNearTheNut()
    {
        Analyze("x 3 2 0 1 0").Difficulty.Should().BeLessThan(Analyze("x 8 7 5 6 5").Difficulty);
    }

    [Test]
    public void Difficulty_IsLowerWithoutABarre()
    {
        Analyze("0 2 2 1 0 0").Difficulty.Should().BeLessThan(Analyze("x 2 4 4 3 2").Difficulty);
    }
}
