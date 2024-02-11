using System;
using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class NoteTests
{
    [Test]
    [TestCase(-1)]
    [TestCase(-2)]
    [TestCase(12)]
    [TestCase(13)]
    public void ThrowsArgumentOutOfRangeException_IndexOutOfRange(int index)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new Note(index));
    }
    
    [Test]
    [TestCase(0, "C")]
    [TestCase(1, "C#")]
    [TestCase(2, "D")]
    [TestCase(3, "D#")]
    [TestCase(4, "E")]
    [TestCase(5, "F")]
    [TestCase(6, "F#")]
    [TestCase(7, "G")]
    [TestCase(8, "G#")]
    [TestCase(9, "A")]
    [TestCase(10, "A#")]
    [TestCase(11, "B")]
    public void ReturnsProperNoteName_ForGivenIndex(int index, string expectedName)
    {
        var note = new Note(index);
        note.ToString().Should().Be(expectedName);
    }

    [Test]
    [TestCase(0, 1, 1)]
    [TestCase(0, 11, 11)]
    [TestCase(0, 12, 0)]
    [TestCase(0, -1, 11)]
    [TestCase(11, -1, 10)]
    [TestCase(11, 1, 0)]
    [TestCase(1, 113, 6)]
    [TestCase(4, 5, 9)]
    public void TransposeReturnsNewNote_WithProperIndex(int startIndex, int interval, int expectedIndex)
    {
        var note = new Note(startIndex);
        var transposedNote = note + new Interval(interval);
        transposedNote.Should().Be(new Note(expectedIndex));
    }

    [Test]
    [TestCase("C𝄫", 10)]
    [TestCase("C♭♭", 10)]
    [TestCase("C♭", 11)]
    [TestCase("Cb", 11)]
    [TestCase("C", 0)]
    [TestCase("C#", 1)]
    [TestCase("  Db", 1)]
    [TestCase("D♭", 1)]
    [TestCase("D𝄫", 0)]
    [TestCase("D", 2)]
    [TestCase("D## ", 4)]
    [TestCase("D###", 5)]
    [TestCase("D#", 3)]
    [TestCase("E      ", 4)]
    [TestCase("E#", 5)]
    [TestCase("F", 5)]
    [TestCase("F#", 6)]
    [TestCase("  G", 7)]
    [TestCase("G#", 8)]
    [TestCase("A", 9)]
    [TestCase( "A#", 10)]
    [TestCase( "D𝄫𝄫", 10)]
    [TestCase( "B", 11)]
    public void Parse_ParsesNote_CorrectString(string noteString, int expectedIndex)
    {
        var note = Note.Parse(noteString);
        note.Should().Be(new Note(expectedIndex));
    }

    [Test]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase(" AB ")]
    [TestCase("Z#")]
    [TestCase("A #")]
    [TestCase("b#")]
    [TestCase("gb")]
    [TestCase("#")]
    [TestCase("b")]
    [TestCase("DG#")]
    public void Parse_ThrowsInvalidOperationException_InvalidString(string noteString)
    {
        Assert.Throws<InvalidOperationException>(() => _ = Note.Parse(noteString));
    }

    [Test]
    [TestCase(0, 1)]
    [TestCase(1, 11)]
    [TestCase(10, 11)]
    [TestCase(4, 5)]
    public void Compare_SecondNoteBigger(int firstNoteIndex, int secondNoteIndex)
    {
        var firstNote = new Note(firstNoteIndex);
        var secondNote = new Note(secondNoteIndex);

        firstNote.CompareTo(secondNote).Should().BeLessThan(0);
        secondNote.CompareTo(firstNote).Should().BeGreaterThan(0);
    }
    
    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(2, 2)]
    [TestCase(4, 4)]
    public void Compare_NotesEqual(int firstNoteIndex, int secondNoteIndex)
    {
        var firstNote = new Note(firstNoteIndex);
        var secondNote = new Note(secondNoteIndex);

        firstNote.CompareTo(secondNote).Should().Be(0);
        secondNote.CompareTo(firstNote).Should().Be(0);
    }
    
    [Test]
    [TestCase(0, 1, 1)]
    [TestCase(0, -1, 11)]
    [TestCase(1, 2, 3)]
    [TestCase(4, 5, 9)]
    [TestCase(10, 5, 3)]
    public void TransposesNote(int noteIndex, int intervalIndex, int expectedNoteIndex)
    {
        var note = new Note(noteIndex);
        var interval = new Interval(intervalIndex);

        var transposedNote = note + interval;
        transposedNote.Should().Be(new Note(expectedNoteIndex));
    }
}
