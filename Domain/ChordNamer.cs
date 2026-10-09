namespace Domain;

/// <summary>
/// Names a chord from the pitches that sound together.
/// </summary>
public static class ChordNamer
{
    // Quality symbol and its semitones above the root. The order is the order of preference
    // when the same notes can be named in more than one way with the same root and bass.
    private static readonly (string Symbol, int[] Semitones)[] Qualities =
    {
        ("", new[] { 0, 4, 7 }),
        ("m", new[] { 0, 3, 7 }),
        ("dim", new[] { 0, 3, 6 }),
        ("aug", new[] { 0, 4, 8 }),
        ("sus2", new[] { 0, 2, 7 }),
        ("sus4", new[] { 0, 5, 7 }),
        ("5", new[] { 0, 7 }),
        ("6", new[] { 0, 4, 7, 9 }),
        ("m6", new[] { 0, 3, 7, 9 }),
        ("7", new[] { 0, 4, 7, 10 }),
        ("maj7", new[] { 0, 4, 7, 11 }),
        ("m7", new[] { 0, 3, 7, 10 }),
        ("mMaj7", new[] { 0, 3, 7, 11 }),
        ("m7b5", new[] { 0, 3, 6, 10 }),
        ("dim7", new[] { 0, 3, 6, 9 }),
        ("7sus4", new[] { 0, 5, 7, 10 }),
        ("aug7", new[] { 0, 4, 8, 10 }),
        ("add9", new[] { 0, 2, 4, 7 }),
        ("madd9", new[] { 0, 2, 3, 7 }),
        ("9", new[] { 0, 2, 4, 7, 10 }),
        ("maj9", new[] { 0, 2, 4, 7, 11 }),
        ("m9", new[] { 0, 2, 3, 7, 10 }),
    };

    private static readonly IReadOnlyDictionary<int, (string Symbol, int Order)> QualityByIntervals =
        Qualities
            .Select((q, order) => (Key: Intervals.Create(q.Semitones.Select(s => new Interval(s))).Value, q.Symbol, Order: order))
            .ToDictionary(t => t.Key, t => (t.Symbol, t.Order));

    /// <summary>
    /// Returns every chord name that fits the given pitches, best first.
    /// Names whose root is the lowest note come before slash chords.
    /// Returns an empty list if the pitches do not form a known chord
    /// (including when fewer than two different notes sound).
    /// </summary>
    public static IReadOnlyList<ChordName> Detect(IEnumerable<Pitch> pitches)
    {
        var sounding = pitches.ToList();
        if (sounding.Count == 0) return Array.Empty<ChordName>();

        var bass = sounding.Min().Note;
        var notes = sounding.Select(p => p.Note).Distinct().ToList();

        var candidates = new List<(ChordName Name, int Order)>();

        foreach (var root in notes)
        {
            var intervals = Intervals.Create(
                notes.Select(n => new Interval(Math.Modulo(n - root, Note.TotalNotes))));

            if (!QualityByIntervals.TryGetValue(intervals.Value, out var quality)) continue;

            candidates.Add((new ChordName(root, quality.Symbol, root == bass ? null : bass), quality.Order));
        }

        return candidates
            .OrderBy(c => c.Name.Bass is null ? 0 : 1)
            .ThenBy(c => c.Order)
            .ThenBy(c => c.Name.Root)
            .Select(c => c.Name)
            .ToList();
    }
}
