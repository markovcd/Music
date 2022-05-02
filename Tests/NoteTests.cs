using System;
using Domain;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class NoteTests
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    [DataRow(-1)]
    [DataRow(-2)]
    [DataRow(12)]
    [DataRow(13)]
    public void ThrowsArgumentOutOfRangeException_IndexOutOfRange(int index)
    {
        _ = new Note(index);
    }
    
    [TestMethod]
    [DataRow(0, "C")]
    [DataRow(1, "C#")]
    [DataRow(2, "D")]
    [DataRow(3, "D#")]
    [DataRow(4, "E")]
    [DataRow(5, "F")]
    [DataRow(6, "F#")]
    [DataRow(7, "G")]
    [DataRow(8, "G#")]
    [DataRow(9, "A")]
    [DataRow(10, "A#")]
    [DataRow(11, "B")]
    public void ReturnsProperNoteName_ForGivenIndex(int index, string expectedName)
    {
        var note = new Note(index);
        note.ToString().Should().Be(expectedName);
    }

    [TestMethod]
    [DataRow(0, 1, 1)]
    [DataRow(0, 11, 11)]
    [DataRow(0, 12, 0)]
    [DataRow(0, -1, 11)]
    [DataRow(11, -1, 10)]
    [DataRow(11, 1, 0)]
    [DataRow(1, 113, 6)]
    [DataRow(4, 5, 9)]
    public void TransposeReturnsNewNote_WithProperIndex(int startIndex, int interval, int expectedIndex)
    {
        var note = new Note(startIndex);
        var transposedNote = note + new Interval(interval);
        transposedNote.Should().Be(new Note(expectedIndex));
    }

    [TestMethod]
    [DataRow("C𝄫", 10)]
    [DataRow("C♭♭", 10)]
    [DataRow("C♭", 11)]
    [DataRow("Cb", 11)]
    [DataRow("C", 0)]
    [DataRow("C#", 1)]
    [DataRow("  Db", 1)]
    [DataRow("D♭", 1)]
    [DataRow("D𝄫", 0)]
    [DataRow("D", 2)]
    [DataRow("D## ", 4)]
    [DataRow("D###", 5)]
    [DataRow("D#", 3)]
    [DataRow("E      ", 4)]
    [DataRow("E#", 5)]
    [DataRow("F", 5)]
    [DataRow("F#", 6)]
    [DataRow("  G", 7)]
    [DataRow("G#", 8)]
    [DataRow("A", 9)]
    [DataRow( "A#", 10)]
    [DataRow( "D𝄫𝄫", 10)]
    [DataRow( "B", 11)]
    public void Parse_ParsesNote_CorrectString(string noteString, int expectedIndex)
    {
        var note = Note.Parse(noteString);
        note.Should().Be(new Note(expectedIndex));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [DataRow("")]
    [DataRow("  ")]
    [DataRow(" AB ")]
    [DataRow("Z#")]
    [DataRow("A #")]
    [DataRow("b#")]
    [DataRow("gb")]
    [DataRow("#")]
    [DataRow("b")]
    [DataRow("DG#")]
    public void Parse_ThrowsInvalidOperationException_InvalidString(string noteString)
    {
        _ = Note.Parse(noteString);
    }

    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(1, 11)]
    [DataRow(10, 11)]
    [DataRow(4, 5)]
    public void Compare_SecondNoteBigger(int firstNoteIndex, int secondNoteIndex)
    {
        var firstNote = new Note(firstNoteIndex);
        var secondNote = new Note(secondNoteIndex);

        firstNote.CompareTo(secondNote).Should().BeLessThan(0);
        secondNote.CompareTo(firstNote).Should().BeGreaterThan(0);
    }
    
    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(1, 1)]
    [DataRow(2, 2)]
    [DataRow(4, 4)]
    public void Compare_NotesEqual(int firstNoteIndex, int secondNoteIndex)
    {
        var firstNote = new Note(firstNoteIndex);
        var secondNote = new Note(secondNoteIndex);

        firstNote.CompareTo(secondNote).Should().Be(0);
        secondNote.CompareTo(firstNote).Should().Be(0);
    }
    
    [TestMethod]
    [DataRow(0, 1, 1)]
    [DataRow(0, -1, 11)]
    [DataRow(1, 2, 3)]
    [DataRow(4, 5, 9)]
    [DataRow(10, 5, 3)]
    public void TransposesNote(int noteIndex, int intervalIndex, int expectedNoteIndex)
    {
        var note = new Note(noteIndex);
        var interval = new Interval(intervalIndex);

        var transposedNote = note + interval;
        transposedNote.Should().Be(new Note(expectedNoteIndex));
    }
}
