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
        
        var chords = ScaleTemplates.Minor.GetChords(ChordTemplates.Seventh).ToList();
    }
}