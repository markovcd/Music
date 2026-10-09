using Domain;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class TuningTemplatesTests
{
    [Test]
    [TestCase("Standard", "Standard (E A D G B E)")]
    [TestCase("Drop D", "Drop D (D A D G B E)")]
    [TestCase("Double drop D", "Double drop D (D A D G B D)")]
    [TestCase("DADGAD", "DADGAD (D A D G A D)")]
    [TestCase("Open G", "Open G (D G D G B D)")]
    [TestCase("Open D", "Open D (D A D F# A D)")]
    [TestCase("Open E", "Open E (E B E G# B E)")]
    [TestCase("Open A", "Open A (E A E A C# E)")]
    [TestCase("Half step down", "Half step down (D# G# C# F# A# D#)")]
    [TestCase("Full step down", "Full step down (D G C F A D)")]
    [TestCase("Bass (4-string)", "Bass (4-string) (E A D G)")]
    [TestCase("Ukulele", "Ukulele (G C E A)")]
    public void All_ListsTheCommonTunings(string name, string label)
    {
        var tuning = TuningTemplates.All.Single(t => t.Name == name);

        tuning.Label.Should().Be(label);
        tuning.ToString().Should().Be(label);
    }

    [Test]
    public void All_HasEachTuningOnce_StandardFirst()
    {
        TuningTemplates.All.Should().HaveCount(12);
        TuningTemplates.All.Select(t => t.Name).Should().OnlyHaveUniqueItems();
        TuningTemplates.All[0].Should().BeSameAs(TuningTemplates.Standard);
    }

    [Test]
    public void Standard_IsTheFretboardStandardTuning()
    {
        TuningTemplates.Standard.Pitches.Should().Equal(Fretboard.StandardTuning);
    }

    [Test]
    public void Tunings_ListTheirStringsFromTheHighestToTheLowest()
    {
        // The ukulele is the exception: its last string is higher than the one before it.
        foreach (var tuning in TuningTemplates.All.Where(t => t.Name != "Ukulele"))
            tuning.Pitches.Select(p => p.Index).Should().BeInDescendingOrder(tuning.Name);

        var ukulele = TuningTemplates.Ukulele.Pitches.Select(p => p.ToString()).ToArray();
        ukulele.Should().Equal("A4", "E4", "C4", "G4");
    }

    [Test]
    public void Tunings_HaveTheNumberOfStringsOfTheirInstrument()
    {
        foreach (var tuning in TuningTemplates.All)
            tuning.Pitches.Count.Should().Be(tuning.Name is "Bass (4-string)" or "Ukulele" ? 4 : 6, tuning.Name);
    }

    [Test]
    public void DropD_OnlyLowersTheLowestStringOfStandard()
    {
        var standard = TuningTemplates.Standard.Pitches;
        var dropD = TuningTemplates.DropD.Pitches;

        dropD.Take(5).Should().Equal(standard.Take(5));
        (standard[5] - dropD[5]).Should().Be(new Interval(2));
    }

    [Test]
    public void HalfStepDown_IsStandardOneSemitoneLower()
    {
        TuningTemplates.HalfStepDown.Pitches.Zip(TuningTemplates.Standard.Pitches, (down, standard) => standard - down)
            .Should().OnlyContain(i => i == new Interval(1));
    }

    [Test]
    public void FullStepDown_IsStandardTwoSemitonesLower()
    {
        TuningTemplates.FullStepDown.Pitches.Zip(TuningTemplates.Standard.Pitches, (down, standard) => standard - down)
            .Should().OnlyContain(i => i == new Interval(2));
    }

    [Test]
    public void Fretboard_CanBeBuiltFromEveryTuning()
    {
        foreach (var tuning in TuningTemplates.All)
        {
            var board = new Fretboard(tuning.Pitches);

            board.StringCount.Should().Be(tuning.Pitches.Count, tuning.Name);
            board.Tunings.Should().Equal(tuning.Pitches);
        }
    }

    [Test]
    public void ChordShapes_OnEveryTuning_AreNamedAsTheChord()
    {
        foreach (var tuning in TuningTemplates.All)
        {
            foreach (var text in new[] { "C", "Am", "G7" })
            {
                var chord = ChordName.Parse(text);

                foreach (var shape in ChordShapeFinder.Find(new Fretboard(tuning.Pitches), chord, maxResults: 20))
                {
                    shape.GetChordNames()
                        .Should().Contain(n => n.Root == chord.Root && n.Quality == chord.Quality && n.Bass == chord.Bass,
                            $"{text} in {tuning.Name}: {shape.Diagram}");
                }
            }
        }
    }

    [Test]
    public void ChordShapes_ChangeWithTheTuning()
    {
        var standard = ChordShapeFinder.Find(Fretboard.Standard(), ChordName.Parse("D"), maxResults: 50).Select(s => s.Diagram);
        var openD = ChordShapeFinder.Find(new Fretboard(TuningTemplates.OpenD.Pitches), ChordName.Parse("D"), maxResults: 50).Select(s => s.Diagram);

        // Open D is tuned to a D major chord, so playing every string open is already D.
        openD.Should().Contain("0 0 0 0 0 0");
        standard.Should().NotContain("0 0 0 0 0 0");
    }

    [Test]
    public void OpenTunings_PlayTheirChordWithEveryStringOpen()
    {
        // Open G and open A have the fifth of the chord (D and E) as their lowest string.
        foreach (var (tuning, name) in new[]
                 {
                     (TuningTemplates.OpenG, "G/D"), (TuningTemplates.OpenD, "D"),
                     (TuningTemplates.OpenE, "E"), (TuningTemplates.OpenA, "A/E"),
                 })
        {
            var board = new Fretboard(tuning.Pitches);
            for (var i = 0; i < board.StringCount; i++) board = board.PressFret(i, 0);

            board.GetChordNames().First().ToString().Should().Be(name, tuning.Name);
        }
    }
}
