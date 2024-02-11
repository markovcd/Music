namespace Domain;

public readonly ref struct Chord(IEnumerable<ChordStep> chordSteps)
{
    public ReadOnlySpan<ChordStep> Steps { get; } = chordSteps.ToArray();
}