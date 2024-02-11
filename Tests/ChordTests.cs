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
        var intervals = new Intervals(1453);
        var chord = intervals.GetChord(2, Degrees.Create(new Degree[] { 1, 3, 5, 7 }));
        var s = chord.Steps.ToImmutableArray();
    }
}