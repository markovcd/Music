using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class ScaleChordTests
{
    private static Note N(string note)
    {
        return Note.Parse(note);
    }

    private static string?[] Names(Scale scale, string root, IReadOnlyList<Degree> template)
    {
        return scale.GetChordNames(N(root), template).Select(n => n?.ToString()).ToArray();
    }

    [Test]
    public void GetChordNames_TriadsOfMinorScale()
    {
        Names(ScaleTemplates.Minor, "A", ChordTemplates.Triad)
            .Should().Equal("Am", "Bdim", "C", "Dm", "Em", "F", "G");
    }

    [Test]
    public void GetChordNames_TriadsOfMajorScale()
    {
        Names(ScaleTemplates.Major, "C", ChordTemplates.Triad)
            .Should().Equal("C", "Dm", "Em", "F", "G", "Am", "Bdim");
    }

    [Test]
    public void GetChordNames_SeventhsOfMinorScale()
    {
        Names(ScaleTemplates.Minor, "A", ChordTemplates.Seventh)
            .Should().Equal("Am7", "Bm7b5", "Cmaj7", "Dm7", "Em7", "Fmaj7", "G7");
    }

    [Test]
    public void GetChordNames_SeventhsOfMajorScale()
    {
        Names(ScaleTemplates.Major, "C", ChordTemplates.Seventh)
            .Should().Equal("Cmaj7", "Dm7", "Em7", "Fmaj7", "G7", "Am7", "Bm7b5");
    }

    [Test]
    public void GetChordNames_SeventhsOfMajorScaleInAnotherKey()
    {
        Names(ScaleTemplates.Major, "G", ChordTemplates.Seventh)
            .Should().Equal("Gmaj7", "Am7", "Bm7", "Cmaj7", "D7", "Em7", "F#m7b5");
    }

    [Test]
    public void GetChordNames_UseConventionalFlatNames()
    {
        Names(ScaleTemplates.Major, "Eb", ChordTemplates.Triad)
            .Should().Equal("Eb", "Fm", "Gm", "Ab", "Bb", "Cm", "Ddim");
    }

    [Test]
    public void GetChordNames_NinthsOfMajorScale_LeavesUnknownChordsNull()
    {
        // Em7b9 and Bm7b5b9 are not chords the namer knows.
        Names(ScaleTemplates.Major, "C", ChordTemplates.Ninth)
            .Should().Equal("Cmaj9", "Dm9", null, "Fmaj9", "G9", "Am9", null);
    }

    [Test]
    public void GetChordNames_SuspendedChords_UseTheNotesOfTheScale()
    {
        // The second of E in C major is F, a semitone up, so there is no Esus2 (or Bsus2).
        Names(ScaleTemplates.Major, "C", ChordTemplates.Suspended2)
            .Should().Equal("Csus2", "Dsus2", null, "Fsus2", "Gsus2", "Asus2", null);

        // The fourth of F in C major is B, an augmented fourth up, so there is no Fsus4 (or Bsus4).
        Names(ScaleTemplates.Major, "C", ChordTemplates.Suspended4)
            .Should().Equal("Csus4", "Dsus4", "Esus4", null, "Gsus4", "Asus4", null);
    }

    [Test]
    public void GetChordNames_ReturnsOneNamePerScaleDegree()
    {
        var pentatonic = Scale.Create(new Interval[] { 0, 2, 4, 7, 9 });

        Names(pentatonic, "C", ChordTemplates.Triad).Should().HaveCount(5);
        Names(ScaleTemplates.Major, "C", ChordTemplates.Triad).Should().HaveCount(7);
    }

    [Test]
    public void GetChordName_NamesTheChordOnOneDegree()
    {
        ScaleTemplates.Major.GetChordName(N("C"), 5, ChordTemplates.Seventh)
            .Should().Be(new ChordName(N("G"), "7"));

        ScaleTemplates.Major.GetChordName(N("C"), 7, ChordTemplates.Triad)
            .Should().Be(new ChordName(N("B"), "dim"));
    }

    [Test]
    public void GetChordName_WithUnknownChord_IsNull()
    {
        ScaleTemplates.Major.GetChordName(N("C"), 3, ChordTemplates.Ninth).Should().BeNull();
    }

    [Test]
    public void GetChordName_WhenOnlyAnotherRootFits_IsNull()
    {
        // C, E and A are Am with its third in the bass, which is not a chord built on C.
        var template = new[] { Degree.First, Degree.Third, Degree.Sixth };

        ScaleTemplates.Major.GetChordName(N("C"), 1, template).Should().BeNull();
    }

    [Test]
    public void GetChordNotes_ReturnsTheNotesInTemplateOrder()
    {
        ScaleTemplates.Major.GetChordNotes(N("C"), 2, ChordTemplates.Seventh)
            .Should().Equal(N("D"), N("F"), N("A"), N("C"));

        ScaleTemplates.Minor.GetChordNotes(N("A"), 7, ChordTemplates.Triad)
            .Should().Equal(N("G"), N("B"), N("D"));
    }

    [Test]
    public void GetChordNotes_StacksNinthsAsTheSecondDegree()
    {
        ScaleTemplates.Major.GetChordNotes(N("C"), 1, ChordTemplates.Ninth)
            .Should().Equal(N("C"), N("E"), N("G"), N("B"), N("D"));
    }

    [Test]
    public void GetChordNotes_WrapsAroundTheScale()
    {
        // Triad on the sixth degree of C major uses degrees 6, 1 and 3.
        ScaleTemplates.Major.GetChordNotes(N("C"), 6, ChordTemplates.Triad)
            .Should().Equal(N("A"), N("C"), N("E"));
    }

    [Test]
    public void GetChordNotes_WithTemplateBeyondTheScale_Throws()
    {
        var pentatonic = Scale.Create(new Interval[] { 0, 2, 4, 7, 9 });

        var act = () => pentatonic.GetChordNotes(N("C"), 1, ChordTemplates.Seventh);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetChordNotes_WithChordRootBeyondTheScale_Throws()
    {
        var act = () => ScaleTemplates.Major.GetChordNotes(N("C"), 8, ChordTemplates.Triad);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetChordNotes_WithEmptyTemplate_Throws()
    {
        var act = () => ScaleTemplates.Major.GetChordNotes(N("C"), 1, Array.Empty<Degree>());

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ChordTemplates_AreStackedFromTheRoot()
    {
        ChordTemplates.Triad.Select(d => (int)d).Should().Equal(1, 3, 5);
        ChordTemplates.Seventh.Select(d => (int)d).Should().Equal(1, 3, 5, 7);
        ChordTemplates.Suspended2.Select(d => (int)d).Should().Equal(1, 2, 5);
        ChordTemplates.Suspended4.Select(d => (int)d).Should().Equal(1, 4, 5);
        ChordTemplates.Ninth.Select(d => (int)d).Should().Equal(1, 3, 5, 7, 2);
        ChordTemplates.Eleventh.Select(d => (int)d).Should().Equal(1, 3, 5, 7, 2, 4);
        ChordTemplates.Thirteenth.Select(d => (int)d).Should().Equal(1, 3, 5, 7, 2, 4, 6);
    }

    [Test]
    [TestCase(1, true)]
    [TestCase(7, true)]
    [TestCase(8, false)]
    public void HasDegree_IsTrueUpToTheNumberOfNotes(int degree, bool expected)
    {
        ScaleTemplates.Major.HasDegree(new Degree((byte)degree)).Should().Be(expected);
    }

    [Test]
    public void GetInterval_ReturnsTheIntervalOfTheDegree()
    {
        ScaleTemplates.Major.GetInterval(1).Should().Be(new Interval(0));
        ScaleTemplates.Major.GetInterval(3).Should().Be(new Interval(4));
        ScaleTemplates.Minor.GetInterval(3).Should().Be(new Interval(3));
        ScaleTemplates.Minor.GetInterval(7).Should().Be(new Interval(10));
    }

    [Test]
    public void GetInterval_BeyondTheScale_Throws()
    {
        var act = () => ScaleTemplates.Major.GetInterval(8);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
