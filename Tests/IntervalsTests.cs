using System;
using System.Collections.Immutable;
using System.Linq;
using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class IntervalsTests
{
  [Test]
  [TestCase(-1)]
  [TestCase(-2)]
  [TestCase(-3)]
  [TestCase(-3000)]
  public void ThrowsArgumentOutOfRangeException_ValueLessThanZero(int value)
  {
    Assert.Throws<ArgumentOutOfRangeException>(() => _ = new Intervals(value));
  }
   
  [Test]
  [TestCase(2741, 0,2,4,5,7,9,11)]
  [TestCase(65, 0,6)]
  [TestCase(4095, 0,1,2,3,4,5,6,7,8,9,10,11)]
  [TestCase(1365, 0,2,4,6,8,10)]
  [TestCase(2137, 0,3,4,6,11)]
  [TestCase(0)]
  [TestCase(1, 0)]
  [TestCase(2, 1)]
  [TestCase(3, 0, 1)]
  public void GetAbsoluteIntervals_GetsIntervalsForGivenIndex(int scaleIndex, params int[] expectedIntervals)
  {
    new Intervals(scaleIndex).ToImmutableArray()
      .Should()
      .BeEquivalentTo(
        expectedIntervals.Select(i => new Interval(i)),
        o => o.WithStrictOrdering());
  }

  [Test]
  public void A()
  {
    var i = Intervals.Create(new[] { 1, 2 }.Select(i => new Interval(i)));
    var x= i - new Interval(2);
  }
}
