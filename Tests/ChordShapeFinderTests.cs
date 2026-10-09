using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class ChordShapeFinderTests
{
    private static Pitch P(string note, int octave)
    {
        return new Pitch(new Octave(octave), Note.Parse(note));
    }

    private static string[] Diagrams(string chord, int maxResults = 500, Fretboard? board = null)
    {
        return ChordShapeFinder.Find(board ?? Fretboard.Standard(), ChordName.Parse(chord), maxResults: maxResults)
            .Select(b => b.Diagram)
            .ToArray();
    }

    private static int?[] Frets(string diagram)
    {
        return diagram.Split(' ').Select(f => f == "x" ? (int?)null : int.Parse(f)).ToArray();
    }

    [Test]
    [TestCase("C", "x 3 2 0 1 0")]
    [TestCase("G", "3 2 0 0 0 3")]
    [TestCase("Am", "x 0 2 2 1 0")]
    [TestCase("D", "x x 0 2 3 2")]
    [TestCase("E", "0 2 2 1 0 0")]
    [TestCase("Em", "0 2 2 0 0 0")]
    [TestCase("F", "1 3 3 2 1 1")]
    [TestCase("Bm", "x 2 4 4 3 2")]
    [TestCase("G7", "3 2 0 0 0 1")]
    [TestCase("Cmaj7", "x 3 2 0 0 0")]
    [TestCase("Dm7", "x x 0 2 1 1")]
    [TestCase("C9", "x 3 2 3 3 0")]
    [TestCase("C/G", "3 3 2 0 1 0")]
    [TestCase("C/E", "0 3 2 0 1 0")]
    [TestCase("E5", "0 2 2 x x x")]
    public void Find_IncludesTheShapesGuitaristsKnow(string chord, string expected)
    {
        Diagrams(chord).Should().Contain(expected);
    }

    [Test]
    [TestCase("C", "x 3 2 0 1 0")]
    [TestCase("G", "3 2 0 0 0 3")]
    public void Find_ListsTheEasiestShapeFirst(string chord, string expected)
    {
        Diagrams(chord).First().Should().Be(expected);
    }

    [Test]
    [TestCase("C")]
    [TestCase("Am")]
    [TestCase("G7")]
    [TestCase("Dm7")]
    [TestCase("F#m7b5")]
    [TestCase("Bbmaj7")]
    [TestCase("Eaug")]
    [TestCase("Gsus4")]
    [TestCase("Cadd9")]
    [TestCase("E5")]
    [TestCase("Edim7")]
    [TestCase("C/G")]
    [TestCase("Am7/G")]
    public void Find_ReturnsShapesThatAreNamedAsTheChord(string text)
    {
        var chord = ChordName.Parse(text);

        var shapes = ChordShapeFinder.Find(Fretboard.Standard(), chord, maxResults: 100);

        shapes.Should().NotBeEmpty();
        foreach (var shape in shapes)
        {
            shape.GetChordNames()
                .Should().Contain(n => n.Root == chord.Root && n.Quality == chord.Quality && n.Bass == chord.Bass,
                    $"{shape.Diagram} should be named {text}");
        }
    }

    [Test]
    [TestCase("C", 4)]
    [TestCase("Am7", 4)]
    [TestCase("G", 3)]
    [TestCase("Bm", 3)]
    [TestCase("F", 5)]
    public void Find_ReturnsOnlyPlayableShapes(string text, int maxSpan)
    {
        var chord = ChordName.Parse(text);
        var shapes = ChordShapeFinder.Find(Fretboard.Standard(), chord, maxSpan, maxResults: 200);

        shapes.Should().NotBeEmpty();

        foreach (var shape in shapes)
        {
            var frets = Frets(shape.Diagram);
            var sounding = Enumerable.Range(0, frets.Length).Where(i => frets[i].HasValue).ToList();
            var fretted = frets.Where(f => f > 0).Select(f => f!.Value).ToList();

            sounding.Count.Should().BeGreaterOrEqualTo(3, shape.Diagram);
            (sounding[^1] - sounding[0] + 1).Should().Be(sounding.Count, $"{shape.Diagram} has a muted string in the middle");
            if (fretted.Count > 0) (fretted.Max() - fretted.Min() + 1).Should().BeLessOrEqualTo(maxSpan, shape.Diagram);
            if (fretted.Count < sounding.Count && fretted.Count > 0) fretted.Max().Should().BeLessOrEqualTo(5, shape.Diagram);
            shape.GetPitches().Min().Note.Should().Be(chord.Bass ?? chord.Root, $"{shape.Diagram} has the wrong bass");
        }
    }

    [Test]
    public void Find_ReturnsEachShapeOnce_OrderedFromTheNutUp()
    {
        var shapes = ChordShapeFinder.Find(Fretboard.Standard(), ChordName.Parse("Am"), maxResults: 500);

        shapes.Select(s => s.Diagram).Should().OnlyHaveUniqueItems();

        var highestFrets = shapes.Select(s => Frets(s.Diagram).Max() ?? 0).ToList();
        highestFrets.Should().BeInAscendingOrder();
    }

    [Test]
    public void Find_StopsAtMaxResults()
    {
        ChordShapeFinder.Find(Fretboard.Standard(), ChordName.Parse("C"), maxResults: 5).Should().HaveCount(5);
        ChordShapeFinder.Find(Fretboard.Standard(), ChordName.Parse("C"), maxResults: 1).Should().HaveCount(1);
        ChordShapeFinder.Find(Fretboard.Standard(), ChordName.Parse("C")).Should().HaveCount(12);
    }

    [Test]
    public void Find_WithMinStrings_OnlyReturnsShapesWithThatManyStrings()
    {
        var shapes = ChordShapeFinder.Find(Fretboard.Standard(), ChordName.Parse("G"), minStrings: 6, maxResults: 100);

        shapes.Should().NotBeEmpty();
        shapes.Should().OnlyContain(s => !s.Diagram.Contains("x"));
    }

    [Test]
    public void Find_WithNarrowerSpan_FindsFewerShapes()
    {
        var wide = ChordShapeFinder.Find(Fretboard.Standard(), ChordName.Parse("C"), maxSpan: 5, maxResults: 500);
        var narrow = ChordShapeFinder.Find(Fretboard.Standard(), ChordName.Parse("C"), maxSpan: 2, maxResults: 500);

        narrow.Count.Should().BeLessThan(wide.Count);
        narrow.Select(s => s.Diagram).Should().BeSubsetOf(wide.Select(s => s.Diagram));
    }

    [Test]
    public void Find_IgnoresFretsAlreadyPressedOnTheBoard()
    {
        var pressed = Fretboard.Standard().PressFret(0, 5).PressFret(3, 7);

        Diagrams("Am", 50, pressed).Should().Equal(Diagrams("Am", 50));
    }

    [Test]
    public void Find_UsesTheBoardTuning()
    {
        // Drop D: the lowest string is D2.
        var dropD = new Fretboard(new[] { P("E", 4), P("B", 3), P("G", 3), P("D", 3), P("A", 2), P("D", 2) });

        Diagrams("D", board: dropD).Should().Contain("0 0 0 2 3 2");
        Diagrams("D").Should().NotContain("0 0 0 2 3 2");
    }

    [Test]
    public void Find_UsesTheBoardFretCount()
    {
        Diagrams("C", board: Fretboard.Standard(3)).Should().BeEmpty();
        Diagrams("C", board: Fretboard.Standard(5)).Should().Contain("x 3 2 0 1 0");
        Diagrams("C", board: Fretboard.Standard(5)).Should().OnlyContain(d => Frets(d).Max() <= 4);
    }

    [Test]
    public void Find_WithTooFewStrings_ReturnsNothing()
    {
        var twoStrings = new Fretboard(new[] { P("E", 4), P("B", 3) });

        Diagrams("C", board: twoStrings).Should().BeEmpty();
    }

    [Test]
    public void Find_ChordsOfFiveNotesMayLeaveOutTheFifth()
    {
        // C9 is C E G Bb D, and x 3 2 3 3 0 has no G.
        var shapes = ChordShapeFinder.Find(Fretboard.Standard(), ChordName.Parse("C9"), maxResults: 500);

        shapes.Select(s => s.Diagram).Should().Contain("x 3 2 3 3 0");
    }

    [Test]
    public void Find_ChordsOfFourNotesMustPlayTheFifth()
    {
        // x 3 2 0 0 0 is Cmaj7; a C7 shape must not be one that drops the G, like x 3 2 3 x x.
        var shapes = ChordShapeFinder.Find(Fretboard.Standard(), ChordName.Parse("C7"), maxResults: 500);

        shapes.Select(s => s.Diagram).Should().NotContain("x 3 2 3 x x");
    }

    [Test]
    public void Find_ShapesForEveryQualityNameThemselvesWhenTheyHaveAFifthToPlay()
    {
        // For chords of up to four notes, the namer must agree with the finder on every shape.
        foreach (var quality in ChordQualities.All.Where(q => q.Tones.Count <= 4))
        {
            var chord = new ChordName(new Note(7), quality.Symbol) { RootName = "G" };

            foreach (var shape in ChordShapeFinder.Find(Fretboard.Standard(), chord, maxResults: 30))
            {
                shape.GetChordNames().Should()
                    .Contain(n => n.Root == chord.Root && n.Quality == chord.Quality, $"G{quality.Symbol}: {shape.Diagram}");
            }
        }
    }

    [Test]
    public void Find_WithInvalidArguments_Throws()
    {
        var board = Fretboard.Standard();
        var chord = ChordName.Parse("C");

        ((Action)(() => ChordShapeFinder.Find(board, chord, maxSpan: 0))).Should().Throw<ArgumentOutOfRangeException>();
        ((Action)(() => ChordShapeFinder.Find(board, chord, minStrings: 0))).Should().Throw<ArgumentOutOfRangeException>();
        ((Action)(() => ChordShapeFinder.Find(board, chord, maxResults: 0))).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Find_WithUnknownQuality_Throws()
    {
        var chord = new ChordName(Note.Parse("C"), "bogus");

        var act = () => ChordShapeFinder.Find(Fretboard.Standard(), chord);

        act.Should().Throw<ArgumentException>();
    }
}
