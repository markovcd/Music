using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class ScaleTemplatesTests
{
    private static int[] Semitones(Scale scale)
    {
        return scale.Intervals.Select(i => (int)i).ToArray();
    }

    [Test]
    public void Major_AndMinor_AreTheTwoOriginalModes()
    {
        Semitones(ScaleTemplates.Major).Should().Equal(0, 2, 4, 5, 7, 9, 11);
        Semitones(ScaleTemplates.Minor).Should().Equal(0, 2, 3, 5, 7, 8, 10);
    }

    [Test]
    public void Modes_AreRotationsOfTheMajorScale()
    {
        Semitones(ScaleTemplates.Dorian).Should().Equal(0, 2, 3, 5, 7, 9, 10);
        Semitones(ScaleTemplates.Phrygian).Should().Equal(0, 1, 3, 5, 7, 8, 10);
        Semitones(ScaleTemplates.Lydian).Should().Equal(0, 2, 4, 6, 7, 9, 11);
        Semitones(ScaleTemplates.Mixolydian).Should().Equal(0, 2, 4, 5, 7, 9, 10);
        Semitones(ScaleTemplates.Locrian).Should().Equal(0, 1, 3, 5, 6, 8, 10);
    }

    [Test]
    public void Pentatonics_HaveFiveNotes()
    {
        Semitones(ScaleTemplates.MajorPentatonic).Should().Equal(0, 2, 4, 7, 9);
        Semitones(ScaleTemplates.MinorPentatonic).Should().Equal(0, 3, 5, 7, 10);
    }

    [Test]
    public void All_NamesEachScaleOnce()
    {
        ScaleTemplates.All.Select(s => s.Name).Should().Equal(
            "Major", "Minor", "Dorian", "Phrygian", "Lydian", "Mixolydian", "Locrian", "Major pentatonic", "Minor pentatonic");

        ScaleTemplates.All.Select(s => s.Name).Should().OnlyHaveUniqueItems();
        ScaleTemplates.All[0].Scale.Should().Be(ScaleTemplates.Major);
        ScaleTemplates.All[1].Scale.Should().Be(ScaleTemplates.Minor);
        ScaleTemplates.All[2].ToString().Should().Be("Dorian");
    }

    [Test]
    public void ModeOfAKey_SharesItsNotesWithTheMajorScale()
    {
        // D Dorian has the notes of C major.
        var notes = ScaleTemplates.Dorian.GetNoteNames(Note.Parse("D"));

        notes.ToString().Should().Be("D E F G A B C");
    }

    [Test]
    public void Modes_AreSpelledWithEachLetterOnce()
    {
        ScaleTemplates.Dorian.GetNoteNames(Note.Parse("Eb")).ToString().Should().Be("Eb F Gb Ab Bb C Db");
        ScaleTemplates.Mixolydian.GetNoteNames(Note.Parse("Bb")).ToString().Should().Be("Bb C D Eb F G Ab");
        ScaleTemplates.Lydian.GetNoteNames(Note.Parse("F")).ToString().Should().Be("F G A B C D E");
    }
}
