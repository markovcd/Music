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

    /// <summary>The given notes as pitches, each above the one before, starting in octave 3.</summary>
    private static Pitch[] Ascending(params string[] notes)
    {
        var pitches = new List<Pitch>();
        var pitch = P(notes[0], 3);
        pitches.Add(pitch);

        foreach (var note in notes.Skip(1))
        {
            do pitch = Pitch.FromIndex(pitch.Index + 1);
            while (pitch.Note != Note.Parse(note));

            pitches.Add(pitch);
        }

        return pitches.ToArray();
    }

    private static string[] Names(params Pitch[] pitches)
    {
        return ChordNamer.Detect(pitches).Select(c => c.ToString()).ToArray();
    }

    // How a root is written: C#/G# for chords with a minor third, Db/Ab for the others.
    private static readonly string[] MajorRootNames =
        { "C", "Db", "D", "Eb", "E", "F", "F#", "G", "Ab", "A", "Bb", "B" };

    private static readonly string[] MinorRootNames =
        { "C", "C#", "D", "Eb", "E", "F", "F#", "G", "G#", "A", "Bb", "B" };

    public static readonly (string Symbol, int[] Semitones)[] AllQualities =
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
        ("6/9", new[] { 0, 2, 4, 7, 9 }),
        ("m6/9", new[] { 0, 2, 3, 7, 9 }),
        ("7", new[] { 0, 4, 7, 10 }),
        ("maj7", new[] { 0, 4, 7, 11 }),
        ("m7", new[] { 0, 3, 7, 10 }),
        ("mMaj7", new[] { 0, 3, 7, 11 }),
        ("m7b5", new[] { 0, 3, 6, 10 }),
        ("dim7", new[] { 0, 3, 6, 9 }),
        ("7b5", new[] { 0, 4, 6, 10 }),
        ("aug7", new[] { 0, 4, 8, 10 }),
        ("maj7#5", new[] { 0, 4, 8, 11 }),
        ("7sus4", new[] { 0, 5, 7, 10 }),
        ("7sus2", new[] { 0, 2, 7, 10 }),
        ("9sus4", new[] { 0, 2, 5, 7, 10 }),
        ("add9", new[] { 0, 2, 4, 7 }),
        ("madd9", new[] { 0, 2, 3, 7 }),
        ("9", new[] { 0, 2, 4, 7, 10 }),
        ("maj9", new[] { 0, 2, 4, 7, 11 }),
        ("m9", new[] { 0, 2, 3, 7, 10 }),
        ("11", new[] { 0, 2, 4, 5, 7, 10 }),
        ("m11", new[] { 0, 2, 3, 5, 7, 10 }),
        ("13", new[] { 0, 2, 4, 7, 9, 10 }),
        ("maj13", new[] { 0, 2, 4, 7, 9, 11 }),
        ("m13", new[] { 0, 2, 3, 7, 9, 10 }),
        ("7b9", new[] { 0, 1, 4, 7, 10 }),
        ("7#9", new[] { 0, 3, 4, 7, 10 }),
        ("7#11", new[] { 0, 4, 6, 7, 10 }),
        ("7b13", new[] { 0, 4, 7, 8, 10 }),
        ("7(no5)", new[] { 0, 4, 10 }),
        ("maj7(no5)", new[] { 0, 4, 11 }),
        ("m7(no5)", new[] { 0, 3, 10 }),
    };

    private static string RootName(int root, string symbol, int[] semitones)
    {
        // The 3 semitones in 7#9 is a raised ninth, not a minor third.
        var hasMinorThird = semitones.Contains(3) && symbol != "7#9";

        return (hasMinorThird ? MinorRootNames : MajorRootNames)[root];
    }

    public static IEnumerable<TestCaseData> EveryQualityOnEveryRoot()
    {
        foreach (var (symbol, semitones) in AllQualities)
        {
            for (var root = 0; root < 12; root++)
                yield return new TestCaseData(root, symbol, semitones)
                    .SetName($"Detect_RootPosition_{RootName(root, symbol, semitones)}{symbol}");
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
        names[0].ToString().Should().Be($"{RootName(root, symbol, semitones)}{symbol}");
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
            .Should().Equal("Cdim7", "Ebdim7/C", "F#dim7/C", "Adim7/C");
    }

    [Test]
    public void Detect_SymmetricAugmentedTriad_ListsEveryRoot()
    {
        Names(P("E", 3), P("G#", 3), P("C", 4)).Should().Equal("Eaug", "Caug/E", "Abaug/E");
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

    [Test]
    [TestCase("D#", "G", "A#", "Eb")]
    [TestCase("A#", "D", "F", "Bb")]
    [TestCase("C#", "F", "G#", "Db")]
    [TestCase("G#", "C", "D#", "Ab")]
    [TestCase("F#", "A#", "C#", "F#")]
    [TestCase("C#", "E", "G#", "C#m")]
    [TestCase("G#", "B", "D#", "G#m")]
    [TestCase("D#", "F#", "A#", "Ebm")]
    [TestCase("A#", "C#", "F", "Bbm")]
    [TestCase("C#", "E", "G", "C#dim")]
    public void Detect_SpellsRootsTheWayChordsAreWritten(string first, string second, string third, string expected)
    {
        Names(Ascending(first, second, third)).Should().Equal(expected);
    }

    [Test]
    public void Detect_SpellsBassNoteAsTheChordToneItIs()
    {
        // The minor third of Cm is Eb, the minor seventh of C7 is Bb, the third of A is C#.
        Names(P("D#", 3), P("G", 3), P("C", 4)).Should().Equal("Cm/Eb");
        Names(P("A#", 2), P("C", 3), P("E", 3), P("G", 3)).Should().Equal("C7/Bb");
        Names(P("D", 3), P("F", 3), P("A#", 3)).Should().Equal("Bb/D");
        Names(P("C#", 3), P("E", 3), P("A", 3)).Should().Equal("A/C#");
        Names(P("C#", 3), P("F#", 3), P("A", 3)).Should().Equal("F#m/C#");
    }

    [Test]
    public void Detect_AvoidsDoubleAccidentalsInTheBass()
    {
        // Strictly the diminished seventh of Eb is Dbb and the augmented fifth of F# is C##.
        Names(P("C", 3), P("D#", 3), P("F#", 3), P("A", 3))
            .Should().Equal("Cdim7", "Ebdim7/C", "F#dim7/C", "Adim7/C");
        Names(P("D", 3), P("F#", 3), P("A#", 3)).Should().Equal("Daug", "F#aug/D", "Bbaug/D");
    }

    [Test]
    public void Detect_ChordWithOmittedFifth_IsNamedWithNo5()
    {
        Names(P("C", 3), P("E", 3), P("A#", 3)).Should().Equal("C7(no5)");
        Names(P("C", 3), P("E", 3), P("B", 3)).Should().Equal("Cmaj7(no5)");
        Names(P("C", 3), P("D#", 3), P("A#", 3)).Should().Equal("Cm7(no5)");
    }

    [Test]
    public void Detect_ChordWithAFifth_IsPreferredOverOneWithout()
    {
        // A, C#, G is A7 without its fifth; with the E it is a normal A7.
        Names(P("A", 2), P("C#", 3), P("G", 3)).Should().Equal("A7(no5)");
        Names(P("A", 2), P("E", 3), P("G", 3), P("C#", 4)).Should().Equal("A7");
    }

    [Test]
    public void Detect_AlteredDominants_AreNamedByTheirAlteration()
    {
        Names(P("G", 2), P("B", 2), P("D", 3), P("F", 3), P("G#", 3)).Should().Equal("G7b9");
        Names(P("E", 2), P("G#", 2), P("B", 2), P("D", 3), P("G", 3)).Should().Equal("E7#9");
        Names(P("C", 3), P("E", 3), P("G", 3), P("A#", 3), P("F#", 4)).Should().Equal("C7#11");
        Names(P("C", 3), P("E", 3), P("G", 3), P("G#", 3), P("A#", 3)).Should().Equal("C7b13");
    }

    [Test]
    public void Detect_ExtendedChords_AreNamedByTheirHighestExtension()
    {
        Names(P("C", 3), P("E", 3), P("G", 3), P("A#", 3), P("D", 4), P("A", 4)).Should().Equal("C13");
        Names(P("C", 3), P("E", 3), P("G", 3), P("B", 3), P("D", 4), P("A", 4)).Should().Equal("Cmaj13", "Am11/C");
    }

    [Test]
    public void Detect_SixNoteChord_CanBeAnInversionOfAnother()
    {
        // C D Eb F G Bb is Cm11, and also Ebmaj13 with its thirteenth (C) in the bass.
        Names(P("C", 3), P("D", 3), P("D#", 3), P("F", 3), P("G", 3), P("A#", 3))
            .Should().Equal("Cm11", "Ebmaj13/C");
    }

    [Test]
    public void Detect_WithNotesAndBass_NamesChordsWithoutOctaves()
    {
        var names = ChordNamer.Detect(new[] { Note.Parse("G"), Note.Parse("B"), Note.Parse("D") }, Note.Parse("B"));

        names.Select(n => n.ToString()).Should().Equal("G/B");
    }

    [Test]
    public void Detect_WithBassThatIsNotOneOfTheNotes_Throws()
    {
        var act = () => ChordNamer.Detect(new[] { Note.Parse("C"), Note.Parse("E"), Note.Parse("G") }, Note.Parse("D"));

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Detect_WithNoNotes_Throws()
    {
        var act = () => ChordNamer.Detect(Array.Empty<Note>(), Note.Parse("C"));

        act.Should().Throw<ArgumentException>();
    }
}
