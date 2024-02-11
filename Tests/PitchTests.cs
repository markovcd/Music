using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class PitchTests
{
   [Test]
   [TestCase(0, 1, 1)]
   [TestCase(1, 1, 13)]
   [TestCase(-1, 11, -1)]
   [TestCase(2, 2, 26)]
   [TestCase(0, 0, 0)]
   [TestCase(0, 11, 11)]
   [TestCase(1, 0, 12)]
   public void Index_ReturnsProperIndex(int octaveIndex, int noteIndex, int pitchIndex)
   {
      var pitch = new Pitch(new Octave(octaveIndex), new Note(noteIndex));
      pitch.Index.Should().Be(pitchIndex);
   }
   
   [Test]
   [TestCase(0, 1, 17.323914436054505)]
   [TestCase(1, 1, 34.64782887210901)]
   [TestCase(-1, 11, 15.433853164253883)]
   [TestCase(2, 2, 73.41619197935188)]
   [TestCase(0, 0, 16.351597831287414)]
   [TestCase(0, 11, 30.86770632850775)]
   [TestCase(1, 0, 32.70319566257483)]
   [TestCase(4, 9, 440)]
   [TestCase(4, 5, 349.2282314330039)]
   [TestCase(3, 9, 220)]
   public void Frequency_ReturnsProperFrequency(int octaveIndex, int noteIndex, double frequency)
   {
      var pitch = new Pitch(new Octave(octaveIndex), new Note(noteIndex));
      pitch.Frequency.Should().Be(new Frequency(frequency));
   }
   
   [Test]
   [TestCase(17.323914436054505, 0, 1)]
   [TestCase(17.32, 0, 1)]
   [TestCase(34.64782887210901, 1, 1)]
   [TestCase(34.648, 1, 1)]
   [TestCase(15.433853164253883, -1, 11)]
   [TestCase(73.41619197935188, 2, 2)]
   [TestCase(16.351597831287414, 0, 0)]
   [TestCase(16.4, 0, 0)]
   [TestCase(30.86770632850775, 0, 11)]
   [TestCase(32.70319566257483, 1, 0)]
   [TestCase(440, 4, 9)]
   [TestCase(349.2282314330039, 4, 5)]
   [TestCase(220, 3, 9)]
   public void FromFrequency_ReturnsProperPitch(double frequency, int octaveIndex, int noteIndex)
   {
      var pitch = Pitch.FromFrequency(new Frequency(frequency));
      pitch.Octave.Should().Be(new Octave(octaveIndex));
      pitch.Note.Should().Be(new Note(noteIndex));
   }

   [Test]
   [TestCase(0, 0, 0)]
   [TestCase(0, 1, 1)]
   [TestCase(1, 1111, 1112)]
   [TestCase(1, -5, -4)]
   public void Transpose_TransposesPitch(int firstIndex, int transposeIndex, int secondIndex)
   {
      var firstPitch = Pitch.FromIndex(firstIndex);
      var secondPitch = firstPitch + new Interval(transposeIndex);
      secondPitch.Should().Be(Pitch.FromIndex(secondIndex));
   }
   
   [Test]
   [TestCase(-2, -1, 10)]
   [TestCase(-1, -1, 11)]
   [TestCase(0, 0, 0)]
   [TestCase(1, 0, 1)]
   [TestCase(2, 0, 2)]
   [TestCase(3, 0, 3)]
   [TestCase(11, 0, 11)]
   [TestCase(12, 1, 0)]
   [TestCase(171, 14, 3)]
   public void FromIndex_ConvertsFromOctaveAndNote(int index, int octave, int note)
   {
      var pitch = Pitch.FromIndex(index);

      pitch.Octave.Should().Be(new Octave(octave));
      pitch.Note.Should().Be(new Note(note));
   }
}
