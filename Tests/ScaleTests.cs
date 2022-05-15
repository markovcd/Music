using Domain;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class ScaleTests
{
  [TestMethod]
  [DataRow(2741, 6, 1453)]
  [DataRow(2137, 1, 2137)]
  [DataRow(2741, 7, 1387)]
  [DataRow(1453, 7, 1717)]
  [DataRow(1453, 7, 1717)]
  [DataRow(1453, 2, 1387)]
  [DataRow(65, 2, 65)]
  [DataRow(2275, 2, 3185)]
  public void Transform_TransformsScaleToGivenDegree(int scale, int degreeIndex, int expectedScale)
  {
    new Scale(new Intervals(scale)).Transform(new Degree(degreeIndex))
      .Should()
      .BeEquivalentTo(new Scale(new Intervals(expectedScale)));
  }
}
