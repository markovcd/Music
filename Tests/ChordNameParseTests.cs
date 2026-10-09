using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class ChordNameParseTests
{
    private static Note N(string note)
    {
        return Note.Parse(note);
    }

    [Test]
    [TestCase("C", "C", "", null)]
    [TestCase("Am", "A", "m", null)]
    [TestCase("Am7", "A", "m7", null)]
    [TestCase("Bbmaj7", "Bb", "maj7", null)]
    [TestCase("F#m7b5", "F#", "m7b5", null)]
    [TestCase("Dbdim7", "Db", "dim7", null)]
    [TestCase("E5", "E", "5", null)]
    [TestCase("G7sus4", "G", "7sus4", null)]
    [TestCase("C7(no5)", "C", "7(no5)", null)]
    [TestCase("C6/9", "C", "6/9", null)]
    [TestCase("Am7/G", "A", "m7", "G")]
    [TestCase("A/C#", "A", "", "C#")]
    [TestCase("Db/F", "Db", "", "F")]
    [TestCase("C6/9/E", "C", "6/9", "E")]
    [TestCase("Bb/Ab", "Bb", "", "Ab")]
    public void Parse_ReadsRootQualityAndBass(string text, string rootName, string quality, string? bassName)
    {
        var chord = ChordName.Parse(text);

        chord.Root.Should().Be(N(rootName));
        chord.RootName.Should().Be(rootName);
        chord.Quality.Should().Be(quality);
        chord.Bass.Should().Be(bassName is null ? null : N(bassName));
        chord.BassName.Should().Be(bassName);
    }

    [Test]
    public void Parse_KeepsTheNamesAsTyped()
    {
        ChordName.Parse("Db").RootName.Should().Be("Db");
        ChordName.Parse("C#").RootName.Should().Be("C#");
        ChordName.Parse("Db").Root.Should().Be(ChordName.Parse("C#").Root);
        ChordName.Parse("Db/Gb").ToString().Should().Be("Db/Gb");
    }

    [Test]
    public void Parse_IgnoresSurroundingWhitespace()
    {
        ChordName.Parse("  Am7 ").ToString().Should().Be("Am7");
    }

    [Test]
    public void Parse_AcceptsUnicodeAccidentals()
    {
        var chord = ChordName.Parse("B♭m/F♯");

        chord.Root.Should().Be(N("Bb"));
        chord.Bass.Should().Be(N("F#"));
    }

    [Test]
    public void Parse_WhenBassIsTheRoot_HasNoBass()
    {
        var chord = ChordName.Parse("C/C");

        chord.Bass.Should().BeNull();
        chord.BassName.Should().BeNull();
        chord.ToString().Should().Be("C");
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("H")]
    [TestCase("c")]
    [TestCase("7")]
    [TestCase("m")]
    [TestCase("Am7x")]
    [TestCase("Cmaj")]
    [TestCase("C/")]
    [TestCase("C/H")]
    [TestCase("C//E")]
    [TestCase("C/E/G")]
    [TestCase("C#b")]
    [TestCase("C6/")]
    [TestCase("Cm/e")]
    [TestCase("Am7 /G")]
    public void TryParse_WithTextThatIsNotAChord_ReturnsFalse(string text)
    {
        ChordName.TryParse(text, out var chord).Should().BeFalse();

        chord.Should().Be(default(ChordName));
        var act = () => ChordName.Parse(text);
        act.Should().Throw<FormatException>();
    }

    [Test]
    public void TryParse_WithNull_ReturnsFalse()
    {
        ChordName.TryParse(null, out _).Should().BeFalse();
    }

    [Test]
    public void Parse_ReadsEveryQualityOnEveryRoot()
    {
        var rootNames = new[] { "C", "Db", "D", "Eb", "E", "F", "F#", "G", "Ab", "A", "Bb", "B" };

        foreach (var quality in ChordQualities.All)
        {
            for (var root = 0; root < 12; root++)
            {
                var text = rootNames[root] + quality.Symbol;

                ChordName.TryParse(text, out var chord).Should().BeTrue(text);
                chord.Root.Should().Be(new Note(root), text);
                chord.Quality.Should().Be(quality.Symbol, text);
                chord.ToString().Should().Be(text);
            }
        }
    }

    [Test]
    public void Parse_ReadsEveryQualityWithABass()
    {
        foreach (var quality in ChordQualities.All)
        {
            var text = "C" + quality.Symbol + "/G";

            ChordName.TryParse(text, out var chord).Should().BeTrue(text);
            chord.Quality.Should().Be(quality.Symbol, text);
            chord.Bass.Should().Be(N("G"), text);
            chord.ToString().Should().Be(text);
        }
    }

    [Test]
    public void Parse_IsTheInverseOfChordNamer()
    {
        var names = ChordNamer.Detect(new[]
        {
            new Pitch(new Octave(2), N("A#")), new Pitch(new Octave(3), N("C")),
            new Pitch(new Octave(3), N("E")), new Pitch(new Octave(3), N("G")),
        });

        names.Should().NotBeEmpty();
        names.Select(n => ChordName.Parse(n.ToString())).Should().Equal(names);
    }
}
