using Domain;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class NoteTests
{
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
    public void TransposeReturnsNewNote_WithProperIndex(int startIndex, int interval, int expectedIndex)
    {
        var note = new Note(startIndex);
        var transposedNote = note + new Interval(interval);
        transposedNote.Should().Be(new Note(expectedIndex));
    }
    
}