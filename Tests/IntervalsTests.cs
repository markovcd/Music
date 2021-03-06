using System;
using System.Collections.Immutable;
using System.Linq;
using Domain;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class IntervalsTests
{
  [TestMethod]
  [ExpectedException(typeof(ArgumentOutOfRangeException))]
  [DataRow(-1)]
  [DataRow(-2)]
  [DataRow(-3)]
  [DataRow(-3000)]
  public void ThrowsArgumentOutOfRangeException_ValueLessThanZero(int value)
  {
    _ = new Intervals(value);
  }
   
  [TestMethod]
  [DataRow(2741, 0,2,4,5,7,9,11)]
  [DataRow(65, 0,6)]
  [DataRow(4095, 0,1,2,3,4,5,6,7,8,9,10,11)]
  [DataRow(1365, 0,2,4,6,8,10)]
  [DataRow(2137, 0,3,4,6,11)]
  [DataRow(0)]
  [DataRow(1, 0)]
  [DataRow(2, 1)]
  [DataRow(3, 0, 1)]
  public void GetAbsoluteIntervals_GetsIntervalsForGivenIndex(int scaleIndex, params int[] expectedIntervals)
  {
    new Intervals(scaleIndex).ToImmutableArray()
      .Should()
      .BeEquivalentTo(
        expectedIntervals.Select(i => new Interval(i)),
        o => o.WithStrictOrdering());
  }

  [TestMethod]
  public void A()
  {
    var i = Intervals.Create(new[] { 1, 2 }.Select(i => new Interval(i)));
    var x= i - new Interval(2);
  }
}
