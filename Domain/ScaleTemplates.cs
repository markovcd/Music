namespace Domain;

public static class ScaleTemplates
{
    public static Scale Minor { get; } = Scale.Create(new Interval[] { 0, 2, 3, 5, 7, 8, 10 });
    public static Scale Major { get; } = Minor.Transform(3);

    // The other modes of the major scale start on its other degrees.
    public static Scale Dorian { get; } = Major.Transform(2);
    public static Scale Phrygian { get; } = Major.Transform(3);
    public static Scale Lydian { get; } = Major.Transform(4);
    public static Scale Mixolydian { get; } = Major.Transform(5);
    public static Scale Locrian { get; } = Major.Transform(7);

    public static Scale MajorPentatonic { get; } = Scale.Create(new Interval[] { 0, 2, 4, 7, 9 });
    public static Scale MinorPentatonic { get; } = Scale.Create(new Interval[] { 0, 3, 5, 7, 10 });

    /// <summary>The scales above with their names, for choosing one from a list.</summary>
    public static IReadOnlyList<NamedScale> All { get; } = new[]
    {
        new NamedScale("Major", Major),
        new NamedScale("Minor", Minor),
        new NamedScale("Dorian", Dorian),
        new NamedScale("Phrygian", Phrygian),
        new NamedScale("Lydian", Lydian),
        new NamedScale("Mixolydian", Mixolydian),
        new NamedScale("Locrian", Locrian),
        new NamedScale("Major pentatonic", MajorPentatonic),
        new NamedScale("Minor pentatonic", MinorPentatonic),
    };
}
