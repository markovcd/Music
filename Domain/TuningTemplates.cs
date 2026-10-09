namespace Domain;

/// <summary>Common tunings. Each lists its strings from the highest to the lowest.</summary>
public static class TuningTemplates
{
    private static Pitch P(string note, int octave)
    {
        return new Pitch(new Octave(octave), Note.Parse(note));
    }

    public static NamedTuning Standard { get; } = new("Standard", Fretboard.StandardTuning);

    public static NamedTuning DropD { get; } = new("Drop D",
        new[] { P("E", 4), P("B", 3), P("G", 3), P("D", 3), P("A", 2), P("D", 2) });

    public static NamedTuning DoubleDropD { get; } = new("Double drop D",
        new[] { P("D", 4), P("B", 3), P("G", 3), P("D", 3), P("A", 2), P("D", 2) });

    public static NamedTuning Dadgad { get; } = new("DADGAD",
        new[] { P("D", 4), P("A", 3), P("G", 3), P("D", 3), P("A", 2), P("D", 2) });

    public static NamedTuning OpenG { get; } = new("Open G",
        new[] { P("D", 4), P("B", 3), P("G", 3), P("D", 3), P("G", 2), P("D", 2) });

    public static NamedTuning OpenD { get; } = new("Open D",
        new[] { P("D", 4), P("A", 3), P("F#", 3), P("D", 3), P("A", 2), P("D", 2) });

    public static NamedTuning OpenE { get; } = new("Open E",
        new[] { P("E", 4), P("B", 3), P("G#", 3), P("E", 3), P("B", 2), P("E", 2) });

    public static NamedTuning OpenA { get; } = new("Open A",
        new[] { P("E", 4), P("C#", 4), P("A", 3), P("E", 3), P("A", 2), P("E", 2) });

    public static NamedTuning HalfStepDown { get; } = new("Half step down",
        new[] { P("D#", 4), P("A#", 3), P("F#", 3), P("C#", 3), P("G#", 2), P("D#", 2) });

    public static NamedTuning FullStepDown { get; } = new("Full step down",
        new[] { P("D", 4), P("A", 3), P("F", 3), P("C", 3), P("G", 2), P("D", 2) });

    public static NamedTuning Bass { get; } = new("Bass (4-string)",
        new[] { P("G", 2), P("D", 2), P("A", 1), P("E", 1) });

    /// <summary>Soprano ukulele. The last string, G, is tuned higher than the C before it.</summary>
    public static NamedTuning Ukulele { get; } = new("Ukulele",
        new[] { P("A", 4), P("E", 4), P("C", 4), P("G", 4) });

    /// <summary>The tunings above, for choosing one from a list. Standard comes first.</summary>
    public static IReadOnlyList<NamedTuning> All { get; } = new[]
    {
        Standard, DropD, DoubleDropD, Dadgad, OpenG, OpenD, OpenE, OpenA, HalfStepDown, FullStepDown, Bass, Ukulele,
    };
}
