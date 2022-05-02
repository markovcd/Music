using Domain;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class PitchTests
{
   [TestMethod]
   [DataRow(0, 1, 1)]
   [DataRow(1, 1, 13)]
   [DataRow(-1, 11, -1)]
   [DataRow(2, 2, 26)]
   [DataRow(0, 0, 0)]
   [DataRow(0, 11, 11)]
   [DataRow(1, 0, 12)]
   public void Index_ReturnsProperIndex(int octaveIndex, int noteIndex, int pitchIndex)
   {
      var pitch = new Pitch(new Octave(octaveIndex), new Note(noteIndex));
      pitch.Index.Should().Be(pitchIndex);
   }
   
   [TestMethod]
   [DataRow(0, 1, 17.323914436054505)]
   [DataRow(1, 1, 34.64782887210901)]
   [DataRow(-1, 11, 15.433853164253883)]
   [DataRow(2, 2, 73.41619197935188)]
   [DataRow(0, 0, 16.351597831287414)]
   [DataRow(0, 11, 30.86770632850775)]
   [DataRow(1, 0, 32.70319566257483)]
   [DataRow(4, 9, 440)]
   [DataRow(4, 5, 349.2282314330039)]
   [DataRow(3, 9, 220)]
   public void Frequency_ReturnsProperFrequency(int octaveIndex, int noteIndex, double frequency)
   {
      var pitch = new Pitch(new Octave(octaveIndex), new Note(noteIndex));
      pitch.Frequency.Should().Be(new Frequency(frequency));
   }
   
   [TestMethod]
   [DataRow(17.323914436054505, 0, 1)]
   [DataRow(17.32, 0, 1)]
   [DataRow(34.64782887210901, 1, 1)]
   [DataRow(34.648, 1, 1)]
   [DataRow(15.433853164253883, -1, 11)]
   [DataRow(73.41619197935188, 2, 2)]
   [DataRow(16.351597831287414, 0, 0)]
   [DataRow(16.4, 0, 0)]
   [DataRow(30.86770632850775, 0, 11)]
   [DataRow(32.70319566257483, 1, 0)]
   [DataRow(440, 4, 9)]
   [DataRow(349.2282314330039, 4, 5)]
   [DataRow(220, 3, 9)]
   public void Frequency_ReturnsProperFrequency(double frequency, int octaveIndex, int noteIndex)
   {
      var pitch = Pitch.FromFrequency(new Frequency(frequency));
      pitch.Octave.Should().Be(new Octave(octaveIndex));
      pitch.Note.Should().Be(new Note(noteIndex));
   }

   [TestMethod]
   [DataRow(0, 0, 0)]
   [DataRow(0, 1, 1)]
   [DataRow(1, 1111, 1112)]
   [DataRow(1, -5, -4)]
   public void Transpose_TransposesPitch(int firstIndex, int transposeIndex, int secondIndex)
   {
      var firstPitch = Pitch.FromIndex(firstIndex);
      var secondPitch = firstPitch + new Interval(transposeIndex);
      secondPitch.Should().Be(Pitch.FromIndex(secondIndex));
   }
   
   [TestMethod]
   public void DDD()
   {
      int firstIndex = 1;
      int transposeIndex = -5;
      int secondIndex = -4;
      
      var firstPitch = Pitch.FromIndex(firstIndex);
      var d = Pitch.FromIndex(secondIndex);
      
      var secondPitch = firstPitch + new Interval(transposeIndex);
      secondPitch.Should().Be(d);
   }
}
