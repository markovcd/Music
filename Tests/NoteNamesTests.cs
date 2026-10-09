using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class NoteNamesTests
{
    private static Note N(string note)
    {
        return Note.Parse(note);
    }

    private static Pitch P(string note, int octave)
    {
        return new Pitch(new Octave(octave), Note.Parse(note));
    }

    [Test]
    [TestCase("C", null, "C D E F G A B")]
    [TestCase("Db", null, "Db Eb F Gb Ab Bb C")]
    [TestCase("C#", "C#", "C# D# E# F# G# A# B#")]
    [TestCase("F#", null, "F# G# A# B C# D# E#")]
    [TestCase("F#", "Gb", "Gb Ab Bb Cb Db Eb F")]
    [TestCase("Ab", null, "Ab Bb C Db Eb F G")]
    [TestCase("G#", "G#", "G# A# B# C# D# E# G")]
    public void GetNoteNames_OfMajorScale_UsesEachLetterOnce(string root, string? rootName, string expected)
    {
        ScaleTemplates.Major.GetNoteNames(N(root), rootName).ToString().Should().Be(expected);
    }

    [Test]
    [TestCase("A", null, "A B C D E F G")]
    [TestCase("C#", null, "C# D# E F# G# A B")]
    [TestCase("Eb", null, "Eb F Gb Ab Bb Cb Db")]
    [TestCase("D#", "D#", "D# E# F# G# A# B C#")]
    public void GetNoteNames_OfMinorScale_UsesEachLetterOnce(string root, string? rootName, string expected)
    {
        ScaleTemplates.Minor.GetNoteNames(N(root), rootName).ToString().Should().Be(expected);
    }

    [Test]
    public void GetNoteNames_OfOtherScales_UsesTheUsualNameOfEachNote()
    {
        var pentatonic = Scale.Create(new Interval[] { 0, 2, 4, 7, 9 });

        pentatonic.GetNoteNames(N("Db")).ToString().Should().Be("Db Eb F Ab Bb");
    }

    [Test]
    public void GetNoteNames_ListsNotesInScaleOrder()
    {
        var names = ScaleTemplates.Major.GetNoteNames(N("Db"));

        names.Select(p => p.Key.ToString()).Should().Equal("C#", "D#", "F", "F#", "G#", "A#", "C");
        names.Select(p => p.Value).Should().Equal("Db", "Eb", "F", "Gb", "Ab", "Bb", "C");
        names.Count.Should().Be(7);
    }

    [Test]
    public void Find_NamesNotesInTheKeyAndNotOthers()
    {
        var names = ScaleTemplates.Major.GetNoteNames(N("Db"));

        names.Find(N("F#")).Should().Be("Gb");
        names.Find(N("C")).Should().Be("C");
        names.Find(N("E")).Should().BeNull();
        names.Find(N("A")).Should().BeNull();
    }

    [Test]
    [TestCase("D")]
    [TestCase("H")]
    [TestCase("")]
    public void GetNoteNames_WithRootNameThatIsNotTheRoot_Throws(string rootName)
    {
        var act = () => ScaleTemplates.Major.GetNoteNames(N("C"), rootName);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Detect_WithNoteNames_SpellsChordsAsTheKeyDoes()
    {
        var names = ScaleTemplates.Major.GetNoteNames(N("Db"));

        ChordNamer.Detect(new[] { P("F#", 3), P("A#", 3), P("C#", 4) }).Select(c => c.ToString()).Should().Equal("F#");
        ChordNamer.Detect(new[] { P("F#", 3), P("A#", 3), P("C#", 4) }, names).Select(c => c.ToString()).Should().Equal("Gb");
    }

    [Test]
    public void Detect_WithNoteNames_SpellsTheBassAsTheKeyDoes()
    {
        var names = ScaleTemplates.Major.GetNoteNames(N("Db"));

        ChordNamer.Detect(new[] { P("A#", 2), P("C#", 3), P("F#", 3) }).Select(c => c.ToString()).Should().Equal("F#/A#");
        ChordNamer.Detect(new[] { P("A#", 2), P("C#", 3), P("F#", 3) }, names).Select(c => c.ToString()).Should().Equal("Gb/Bb");
    }

    [Test]
    public void Detect_WithNoteNames_KeepsTheUsualSpellingForNotesOutsideTheKey()
    {
        var names = ScaleTemplates.Major.GetNoteNames(N("Db"));

        // A is not in Db major, so A major is still A.
        ChordNamer.Detect(new[] { P("A", 3), P("C#", 4), P("E", 4) }, names).Select(c => c.ToString()).First().Should().Be("A");
    }

    [Test]
    public void Detect_WithNotesAndBass_AcceptsNoteNames()
    {
        var names = ScaleTemplates.Major.GetNoteNames(N("Db"));

        ChordNamer.Detect(new[] { N("F#"), N("A#"), N("C#") }, N("F#"), names)
            .Select(c => c.ToString()).Should().Equal("Gb");
    }

    [Test]
    public void Fretboard_GetChordNames_UsesNoteNames()
    {
        var board = Fretboard.Standard();
        // F# barre chord: 2 4 4 3 2 2 from the lowest string.
        var frets = new[] { 2, 4, 4, 3, 2, 2 };
        for (var i = 0; i < frets.Length; i++)
            board = board.PressFret(frets.Length - 1 - i, frets[i]);

        board.GetChordNames().Select(c => c.ToString()).Should().Equal("F#");
        board.GetChordNames(ScaleTemplates.Major.GetNoteNames(N("Db"))).Select(c => c.ToString()).Should().Equal("Gb");
    }

    [Test]
    public void Scale_GetChordNames_SpellsChordsAsTheKeyDoes()
    {
        Names(ScaleTemplates.Major, "Db", null, ChordTemplates.Triad)
            .Should().Equal("Db", "Ebm", "Fm", "Gb", "Ab", "Bbm", "Cdim");
        Names(ScaleTemplates.Major, "F#", null, ChordTemplates.Seventh)
            .Should().Equal("F#maj7", "G#m7", "A#m7", "Bmaj7", "C#7", "D#m7", "E#m7b5");
        Names(ScaleTemplates.Minor, "C#", null, ChordTemplates.Triad)
            .Should().Equal("C#m", "D#dim", "E", "F#m", "G#m", "A", "B");
        Names(ScaleTemplates.Minor, "Eb", null, ChordTemplates.Triad)
            .Should().Equal("Ebm", "Fdim", "Gb", "Abm", "Bbm", "Cb", "Db");
    }

    [Test]
    public void Scale_GetChordNames_WithRootName_PicksTheSpellingOfTheKey()
    {
        Names(ScaleTemplates.Major, "F#", "Gb", ChordTemplates.Triad)
            .Should().Equal("Gb", "Abm", "Bbm", "Cb", "Db", "Ebm", "Fdim");
    }

    [Test]
    public void Scale_GetChordName_SpellsTheChordAsTheKeyDoes()
    {
        var name = ScaleTemplates.Major.GetChordName(N("Db"), 4, ChordTemplates.Triad);

        name.Should().NotBeNull();
        name!.Value.ToString().Should().Be("Gb");
    }

    private static string?[] Names(Scale scale, string root, string? rootName, IReadOnlyList<Degree> template)
    {
        return scale.GetChordNames(N(root), template, rootName).Select(n => n?.ToString()).ToArray();
    }
}
