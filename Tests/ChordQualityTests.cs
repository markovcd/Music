using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class ChordQualityTests
{
    [Test]
    [TestCase("1 3 5", new[] { 0, 4, 7 }, new[] { 0, 2, 4 })]
    [TestCase("1 b3 5 b7", new[] { 0, 3, 7, 10 }, new[] { 0, 2, 4, 6 })]
    [TestCase("1 b3 b5 bb7", new[] { 0, 3, 6, 9 }, new[] { 0, 2, 4, 6 })]
    [TestCase("1 3 #5 7", new[] { 0, 4, 8, 11 }, new[] { 0, 2, 4, 6 })]
    [TestCase("1 5", new[] { 0, 7 }, new[] { 0, 4 })]
    [TestCase("1 2 5", new[] { 0, 2, 7 }, new[] { 0, 1, 4 })]
    [TestCase("1 4 5", new[] { 0, 5, 7 }, new[] { 0, 3, 4 })]
    [TestCase("1 3 5 6", new[] { 0, 4, 7, 9 }, new[] { 0, 2, 4, 5 })]
    [TestCase("1 3 5 b7 9", new[] { 0, 4, 7, 10, 2 }, new[] { 0, 2, 4, 6, 1 })]
    [TestCase("1 3 5 b7 b9", new[] { 0, 4, 7, 10, 1 }, new[] { 0, 2, 4, 6, 1 })]
    [TestCase("1 3 5 b7 #9", new[] { 0, 4, 7, 10, 3 }, new[] { 0, 2, 4, 6, 1 })]
    [TestCase("1 3 5 b7 9 11", new[] { 0, 4, 7, 10, 2, 5 }, new[] { 0, 2, 4, 6, 1, 3 })]
    [TestCase("1 3 5 b7 #11", new[] { 0, 4, 7, 10, 6 }, new[] { 0, 2, 4, 6, 3 })]
    [TestCase("1 3 5 b7 9 13", new[] { 0, 4, 7, 10, 2, 9 }, new[] { 0, 2, 4, 6, 1, 5 })]
    [TestCase("1 3 5 b7 b13", new[] { 0, 4, 7, 10, 8 }, new[] { 0, 2, 4, 6, 5 })]
    [TestCase("1   3\t5", new[] { 0, 4, 7 }, new[] { 0, 2, 4 })]
    public void Constructor_ReadsTheFormula(string formula, int[] semitones, int[] letterSteps)
    {
        var quality = new ChordQuality("x", formula);

        quality.Formula.Should().Be(formula);
        quality.Tones.Select(t => t.Semitones).Should().Equal(semitones);
        quality.Tones.Select(t => t.LetterSteps).Should().Equal(letterSteps);
    }

    [Test]
    public void Constructor_SetsSymbolAndIntervals()
    {
        var quality = new ChordQuality("m7", "1 b3 5 b7");

        quality.Symbol.Should().Be("m7");
        quality.Intervals.Select(i => (int)i).Should().Equal(0, 3, 7, 10);
        quality.ToString().Should().Be("m7 (1 b3 5 b7)");
    }

    [Test]
    public void Constructor_AllowsAnEmptySymbol()
    {
        new ChordQuality("", "1 3 5").Symbol.Should().BeEmpty();
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("3 5")]
    [TestCase("b1 3 5")]
    [TestCase("1 3 x")]
    [TestCase("1 3 5 8")]
    [TestCase("1 3 5 10")]
    [TestCase("1 3 5 0")]
    [TestCase("1 3 5 99999999999")]
    [TestCase("1 3 3")]
    [TestCase("1 2 9")]
    [TestCase("1 3 5 #")]
    [TestCase("1 3 5 7b")]
    public void Constructor_WithInvalidFormula_Throws(string formula)
    {
        var act = () => new ChordQuality("x", formula);

        act.Should().Throw<FormatException>();
    }

    [Test]
    public void Constructor_WithoutSymbolOrFormula_Throws()
    {
        var withoutSymbol = () => new ChordQuality(null!, "1 3 5");
        var withoutFormula = () => new ChordQuality("m", null!);

        withoutSymbol.Should().Throw<ArgumentNullException>();
        withoutFormula.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void All_ListsEveryQualityTheNamerIsTestedWith()
    {
        ChordQualities.All.Select(q => q.Symbol)
            .Should().Equal(ChordNamerTests.AllQualities.Select(q => q.Symbol));

        ChordQualities.All.Select(q => q.Tones.Select(t => t.Semitones).OrderBy(s => s).ToArray())
            .Should().BeEquivalentTo(
                ChordNamerTests.AllQualities.Select(q => q.Semitones.OrderBy(s => s).ToArray()),
                o => o.WithStrictOrdering());
    }

    [Test]
    public void All_HasNoTwoQualitiesWithTheSameSymbolOrNotes()
    {
        ChordQualities.All.Select(q => q.Symbol).Should().OnlyHaveUniqueItems();
        ChordQualities.All.Select(q => q.Intervals.Value).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void All_StartsEveryChordOnTheRoot()
    {
        ChordQualities.All.Should().OnlyContain(q => q.Tones[0].Semitones == 0 && q.Tones[0].LetterSteps == 0);
    }
}
