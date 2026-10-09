using System.Collections.Immutable;

namespace Domain;

/// <summary>
/// Which scale degrees, counted from the chord's root, a chord is stacked from.
/// Degrees above the octave are written as their scale degree, so the ninth is the second (<see cref="Degree.Ninth"/>).
/// </summary>
public static class ChordTemplates
{
    public static IReadOnlyList<Degree> Triad { get; }
        = ImmutableArray.Create(Degree.First, Degree.Third, Degree.Fifth);

    public static IReadOnlyList<Degree> Seventh { get; }
        = ImmutableArray.Create(Degree.First, Degree.Third, Degree.Fifth, Degree.Seventh);

    public static IReadOnlyList<Degree> Suspended2 { get; }
        = ImmutableArray.Create(Degree.First, Degree.Second, Degree.Fifth);

    public static IReadOnlyList<Degree> Suspended4 { get; }
        = ImmutableArray.Create(Degree.First, Degree.Fourth, Degree.Fifth);

    public static IReadOnlyList<Degree> Ninth { get; }
        = ImmutableArray.Create(Degree.First, Degree.Third, Degree.Fifth, Degree.Seventh, Degree.Ninth);

    public static IReadOnlyList<Degree> Eleventh { get; }
        = ImmutableArray.Create(Degree.First, Degree.Third, Degree.Fifth, Degree.Seventh, Degree.Ninth, Degree.Eleventh);

    public static IReadOnlyList<Degree> Thirteenth { get; }
        = ImmutableArray.Create(Degree.First, Degree.Third, Degree.Fifth, Degree.Seventh, Degree.Ninth, Degree.Eleventh, Degree.Thirteenth);
}
