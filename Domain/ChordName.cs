namespace Domain;

/// <summary>
/// A chord symbol such as <c>C</c>, <c>Am7</c> or <c>G/B</c>.
/// <see cref="Bass"/> is only set for slash chords (when the lowest note is not the root).
/// </summary>
public readonly record struct ChordName(Note Root, string Quality, Note? Bass = null)
{
    public override string ToString()
    {
        return Bass is { } bass && bass != Root
            ? $"{Root}{Quality}/{bass}"
            : $"{Root}{Quality}";
    }
}
