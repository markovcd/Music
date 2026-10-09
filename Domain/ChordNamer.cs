namespace Domain;

/// <summary>
/// Names a chord from the notes that sound together, using the qualities in <see cref="ChordQualities"/>.
/// Roots are spelled in the conventional way (Eb and Bb, but F# and C#m), and a slash chord's bass note is
/// spelled as the chord tone it is, so the third of Eb is written G and the third of Cm is written Eb.
/// Pass the <see cref="NoteNames"/> of a key to spell the notes of that key the way the key does:
/// in Db major the chord on the fourth degree is Gb, not F#. Notes outside the key keep the usual spelling.
/// </summary>
public static class ChordNamer
{
    private static readonly IReadOnlyDictionary<int, (ChordQuality Quality, int Order)> QualityByIntervals =
        ChordQualities.All
            .Select((quality, order) => (Quality: quality, Order: order))
            .ToDictionary(t => t.Quality.Intervals.Value);

    /// <summary>
    /// Returns every chord name that fits the given pitches, best first.
    /// Names whose root is the lowest note come before slash chords.
    /// Returns an empty list if the pitches do not form a known chord
    /// (including when fewer than two different notes sound).
    /// </summary>
    public static IReadOnlyList<ChordName> Detect(IEnumerable<Pitch> pitches, NoteNames? names = null)
    {
        var sounding = pitches.ToList();
        if (sounding.Count == 0) return Array.Empty<ChordName>();

        return Detect(sounding.Select(p => p.Note), sounding.Min().Note, names);
    }

    /// <summary>
    /// Like <see cref="Detect(IEnumerable{Pitch}, NoteNames)"/> for notes without octaves.
    /// <paramref name="bass"/> is the lowest note and must be one of <paramref name="notes"/>.
    /// </summary>
    public static IReadOnlyList<ChordName> Detect(IEnumerable<Note> notes, Note bass, NoteNames? names = null)
    {
        var distinct = notes.Distinct().ToList();

        if (!distinct.Contains(bass))
            throw new ArgumentException("The bass note must be one of the notes.", nameof(bass));

        var candidates = new List<(ChordName Name, int Order)>();

        foreach (var root in distinct)
        {
            var intervals = Intervals.Create(
                distinct.Select(n => new Interval(Math.Modulo(n - root, Note.TotalNotes))));

            if (!QualityByIntervals.TryGetValue(intervals.Value, out var match)) continue;

            candidates.Add((CreateName(root, match.Quality, bass, names), match.Order));
        }

        return candidates
            .OrderBy(c => c.Name.Bass is null ? 0 : 1)
            .ThenBy(c => c.Order)
            .ThenBy(c => c.Name.Root)
            .Select(c => c.Name)
            .ToList();
    }

    private static ChordName CreateName(Note root, ChordQuality quality, Note bass, NoteNames? names)
    {
        var rootName = names?.Find(root) ?? NoteSpelling.SpellRoot(root, quality.HasMinorThird);

        if (root == bass)
            return new ChordName(root, quality.Symbol) { RootName = rootName };

        var bassTone = quality.Tones.First(t => root + new Interval(t.Semitones) == bass);

        return new ChordName(root, quality.Symbol, bass)
        {
            RootName = rootName,
            BassName = names?.Find(bass)
                       ?? NoteSpelling.SpellTone(rootName, bassTone.LetterSteps, bass, quality.HasMinorThird),
        };
    }
}
