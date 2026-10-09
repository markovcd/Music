namespace Domain;

/// <summary>
/// The tuning of a stringed instrument with a name to show, such as "Drop D".
/// The pitches are listed from the highest string to the lowest, like <see cref="Fretboard.StandardTuning"/>.
/// </summary>
public sealed record NamedTuning(string Name, IReadOnlyList<Pitch> Pitches)
{
    /// <summary>The name and the notes of the strings from the last one to the first, for example "Drop D (D A D G B E)".</summary>
    public string Label => $"{Name} ({string.Join(" ", Pitches.Reverse().Select(p => p.Note))})";

    public override string ToString()
    {
        return Label;
    }
}
