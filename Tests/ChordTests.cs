using System.Collections.Immutable;
using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class ChordTests
{
    [Test]
    public void T()
    {
        var scale = new Scale(new Intervals(1453));
        var chord = scale.GetChord(2, ScaleDegrees.Create(new ScaleDegree[] { 1, 3, 5, 7 }));
        var a = chord.GetInterval(2);
        var b = chord.GetInterval(3);
        var c = chord.GetInterval(5);
        var d = chord.GetInterval(7);
    }
}