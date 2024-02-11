using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class ScaleTests
{
  [Test]
  [TestCase(2741, 6, 1453)]
  [TestCase(2137, 1, 2137)]
  [TestCase(2741, 7, 1387)]
  [TestCase(1453, 7, 1717)]
  [TestCase(1453, 7, 1717)]
  [TestCase(1453, 2, 1387)]
  [TestCase(65, 2, 65)]
  [TestCase(2275, 2, 3185)]
  public void Transform_TransformsScaleToGivenDegree(int scale, int degreeIndex, int expectedScale)
  {
    new Scale(new Intervals(scale)).Transform(new Degree(degreeIndex))
      .Should()
      .BeEquivalentTo(new Scale(new Intervals(expectedScale)));
  }
}
