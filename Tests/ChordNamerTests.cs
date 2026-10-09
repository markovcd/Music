using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class ChordNamerTests
{
    private static Pitch P(string note, int octave)
    {
        return new Pitch(new Octave(octave), Note.Parse(note));
    }

    private static string[] Names(params Pitch[] pitches)
    {
        return ChordNamer.Detect(pitches).Select(c => c.ToString()).ToArray();
    }

    private static readonly string[] RootNames =
        { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    private static readonly (string Symbol, int[] Semitones)[] AllQualities =
    {
        ("", new[] { 0, 4, 7 }),
        ("m", new[] { 0, 3, 7 }),
        ("dim", new[] { 0, 3, 6 }),
        ("aug", new[] { 0, 4, 8 }),
        ("sus2", new[] { 0, 2, 7 }),
        ("sus4", new[] { 0, 5, 7 }),
        ("5", new[] { 0, 7 }),
        ("6", new[] { 0, 4, 7, 9 }),
        ("m6", new[] { 0, 3, 7, 9 }),
        ("7", new[] { 0, 4, 7, 10 }),
        ("maj7", new[] { 0, 4, 7, 11 }),
        ("m7", new[] { 0, 3, 7, 10 }),
        ("mMaj7", new[] { 0, 3, 7, 11 }),
        ("m7b5", new[] { 0, 3, 6, 10 }),
        ("dim7", new[] { 0, 3, 6, 9 }),
        ("7sus4", new[] { 0, 5, 7, 10 }),
        ("aug7", new[] { 0, 4, 8, 10 }),
        ("add9", new[] { 0, 2, 4, 7 }),
        ("madd9", new[] { 0, 2, 3, 7 }),
        ("9", new[] { 0, 2, 4, 7, 10 }),
        ("maj9", new[] { 0, 2, 4, 7, 11 }),
        ("m9", new[] { 0, 2, 3, 7, 10 }),
    };

    public static IEnumerable<TestCaseData> EveryQualityOnEveryRoot()
    {
        foreach (var (symbol, semitones) in AllQualities)
        {
            for (var root = 0; root < 12; root++)
                yield return new TestCaseData(root, symbol, semitones)
                    .SetName($"Detect_RootPosition_{RootNames[root]}{symbol}");
        }
    }

    [Test]
    public void Detect_NoPitches_ReturnsNoChords()
    {
        ChordNamer.Detect(Array.Empty<Pitch>()).Should().BeEmpty();
    }

    [Test]
    public void Detect_SingleNote_ReturnsNoChords()
    {
        Names(P("C", 3)).Should().BeEmpty();
    }

    [Test]
    public void Detect_SameNoteInSeveralOctaves_ReturnsNoChords()
    {
        Names(P("E", 2), P("E", 3), P("E", 4)).Should().BeEmpty();
    }

    [Test]
    [TestCaseSource(nameof(EveryQualityOnEveryRoot))]
    public void Detect_RootPosition_NamesTheChordFirst(int root, string symbol, int[] semitones)
    {
        var rootPitch = new Pitch(new Octave(3), new Note(root));
        var pitches = semitones.Select(s => rootPitch + new Interval(s));

        var names = ChordNamer.Detect(pitches);

        names.Should().NotBeEmpty();
        names[0].ToString().Should().Be($"{RootNames[root]}{symbol}");
        names[0].Bass.Should().BeNull();
    }

    [Test]
    public void Detect_IgnoresOrderOfPitches()
    {
        var ascending = Names(P("C", 3), P("E", 3), P("G", 3));
        var shuffled = Names(P("G", 3), P("C", 3), P("E", 3));

        shuffled.Should().Equal(ascending);
    }

    [Test]
    public void Detect_DoubledNotes_AreTreatedAsOne()
    {
        Names(P("C", 3), P("E", 3), P("G", 3), P("C", 4), P("E", 4)).Should().Equal("C");
    }

    [Test]
    public void Detect_NoteSpreadOverManyOctaves_StillNamesTheChord()
    {
        Names(P("C", 1), P("G", 2), P("E", 5)).Should().Equal("C");
    }

    [Test]
    public void Detect_ThirdInBass_ReturnsSlashChord()
    {
        var names = ChordNamer.Detect(new[] { P("E", 2), P("G", 2), P("C", 3) });

        names.Should().Equal(new ChordName(Note.Parse("C"), "", Note.Parse("E")));
        names[0].ToString().Should().Be("C/E");
    }

    [Test]
    public void Detect_FifthInBass_ReturnsSlashChord()
    {
        Names(P("G", 2), P("C", 3), P("E", 3)).Should().Equal("C/G");
    }

    [Test]
    public void Detect_UsesLowestPitchAsBassNotLowestNote()
    {
        // G is the lowest pitch even though C is the lowest note name.
        Names(P("C", 4), P("E", 4), P("G", 3)).Should().Equal("C/G");
    }

    [Test]
    public void Detect_AmbiguousChord_ListsRootPositionBeforeSlashChords()
    {
        // C E G A is C6, and also Am7 with C (its minor third) in the bass.
        Names(P("C", 3), P("E", 3), P("G", 3), P("A", 3)).Should().Equal("C6", "Am7/C");
    }

    [Test]
    public void Detect_AmbiguousChordWithOtherBass_ListsTheMatchingRootFirst()
    {
        Names(P("A", 2), P("C", 3), P("E", 3), P("G", 3)).Should().Equal("Am7", "C6/A");
    }

    [Test]
    public void Detect_SymmetricDiminishedSeventh_ListsEveryRoot()
    {
        Names(P("C", 3), P("D#", 3), P("F#", 3), P("A", 3))
            .Should().Equal("Cdim7", "D#dim7/C", "F#dim7/C", "Adim7/C");
    }

    [Test]
    public void Detect_SymmetricAugmentedTriad_ListsEveryRoot()
    {
        Names(P("E", 3), P("G#", 3), P("C", 4)).Should().Equal("Eaug", "Caug/E", "G#aug/E");
    }

    [Test]
    public void Detect_Sus2AndSus4OfAnotherRoot_PreferTheRootInTheBass()
    {
        Names(P("C", 3), P("D", 3), P("G", 3)).Should().Equal("Csus2", "Gsus4/C");
        Names(P("G", 2), P("C", 3), P("D", 3)).Should().Equal("Gsus4", "Csus2/G");
    }

    [Test]
    public void Detect_PowerChord_IsNamedWithFive()
    {
        Names(P("E", 2), P("B", 2), P("E", 3)).Should().Equal("E5");
    }

    [Test]
    public void Detect_NotesThatFormNoKnownChord_ReturnsNoChords()
    {
        Names(P("C", 3), P("C#", 3), P("D", 3)).Should().BeEmpty();
    }

    [Test]
    public void Detect_ChordWithMissingThird_IsNotGuessed()
    {
        // C and Bb alone are a minor seventh interval, not a chord we know.
        Names(P("C", 3), P("A#", 3)).Should().BeEmpty();
    }

    [Test]
    public void ChordName_ToString_OmitsBassWhenItIsTheRoot()
    {
        new ChordName(Note.Parse("A"), "m7").ToString().Should().Be("Am7");
        new ChordName(Note.Parse("A"), "m7", Note.Parse("A")).ToString().Should().Be("Am7");
    }

    [Test]
    public void ChordName_ToString_AppendsBassForSlashChords()
    {
        new ChordName(Note.Parse("G"), "", Note.Parse("B")).ToString().Should().Be("G/B");
        new ChordName(Note.Parse("F#"), "m", Note.Parse("C#")).ToString().Should().Be("F#m/C#");
    }
}
