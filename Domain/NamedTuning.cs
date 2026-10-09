namespace Domain;

/// <summary>
/// The tuning of a stringed instrument with a name to show, such as "Drop D".
/// The pitches are listed from the highest string to the lowest, like <see cref="Fretboard.StandardTuning"/>.
/// </summary>
public sealed record NamedTuning(string Name, IReadOnlyList<Pitch> Pitches)
{
    /// <summary>The name and the notes of the strings from the last one to the first, for example "Drop D (D A D G B E)".</summary>
    public string Label => $"{Name} ({string.Join(" ", Pitches.Reverse().Select(p => p.Note))})";

    /// <summary>
    /// The pitches of the open strings with a capo on the given fret, which raises every string by that many semitones.
    /// Frets are then counted from the capo, so fret 0 is the string held down by the capo.
    /// </summary>
    public IReadOnlyList<Pitch> PitchesWithCapo(int fret)
    {
        if (fret < 0) throw new ArgumentOutOfRangeException(nameof(fret), fret, null);

        return Pitches.Select(p => p + new Interval(fret)).ToList();
    }

    public override string ToString()
    {
        return Label;
    }
}
