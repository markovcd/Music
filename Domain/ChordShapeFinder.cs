namespace Domain;

/// <summary>
/// Finds ways to play a chord on a fretboard: the reverse of <see cref="ChordNamer"/>.
/// </summary>
public static class ChordShapeFinder
{
    // Open strings are only allowed with fretted notes in the first few frets,
    // so a shape like "open string + notes at the ninth fret" is not offered.
    private const int OpenStringReach = 5;

    /// <summary>
    /// Returns playable shapes for the chord on the board's tuning, easiest first
    /// (lowest on the neck, then most strings, then narrowest).
    /// </summary>
    /// <remarks>
    /// A shape sounds one fret per string or mutes the string, fits within <paramref name="maxSpan"/> frets (open strings
    /// do not count), has no muted string between sounding ones, plays every note of the chord and has the chord's root
    /// (or the note after the slash) as its lowest note. Chords of five or more notes may leave out the fifth.
    /// </remarks>
    /// <param name="board">The strings and number of frets to use. Frets already pressed on it are ignored.</param>
    /// <param name="chord">The chord, for example <c>ChordName.Parse("Am7")</c>.</param>
    /// <param name="maxSpan">The most frets a shape may cover, counting the first and last fretted one.</param>
    /// <param name="minStrings">The fewest strings that must sound.</param>
    /// <param name="maxResults">The most shapes to return.</param>
    public static IReadOnlyList<Fretboard> Find(
        Fretboard board,
        ChordName chord,
        int maxSpan = 4,
        int minStrings = 3,
        int maxResults = 12)
    {
        if (maxSpan < 1) throw new ArgumentOutOfRangeException(nameof(maxSpan), maxSpan, null);
        if (minStrings < 1) throw new ArgumentOutOfRangeException(nameof(minStrings), minStrings, null);
        if (maxResults < 1) throw new ArgumentOutOfRangeException(nameof(maxResults), maxResults, null);

        var quality = ChordQualities.Find(chord.Quality)
            ?? throw new ArgumentException($"\"{chord.Quality}\" is not a known chord quality.", nameof(chord));

        var chordNotes = quality.Tones.Select(t => chord.Root + new Interval(t.Semitones)).ToList();
        var bass = chord.Bass ?? chord.Root;

        var allowed = chordNotes.Append(bass).ToHashSet();
        var required = new HashSet<Note>(allowed);

        if (quality.Tones.Count >= 5)
            required.Remove(chord.Root + Interval.Fifth);

        required.Add(chord.Root);
        required.Add(bass);

        var search = new Search(board, allowed, required, bass, maxSpan, minStrings);
        var empty = new Fretboard(board.Tunings, board.FretCount);

        return search.Run()
            .OrderBy(s => s.MaxFret)
            .ThenByDescending(s => s.Sounding)
            .ThenBy(s => s.Span)
            .ThenBy(s => s.Key, StringComparer.Ordinal)
            .Take(maxResults)
            .Select(s => s.Frets.Select((fret, i) => (fret, i))
                .Where(t => t.fret.HasValue)
                .Aggregate(empty, (b, t) => b.PressFret(t.i, t.fret!.Value)))
            .ToList();
    }

    private sealed record Shape(int?[] Frets, int MaxFret, int Sounding, int Span, string Key);

    private sealed class Search
    {
        private readonly IReadOnlyList<Pitch> tunings;
        private readonly int fretCount;
        private readonly HashSet<Note> allowed;
        private readonly HashSet<Note> required;
        private readonly Note bass;
        private readonly int maxSpan;
        private readonly int minStrings;

        private readonly Dictionary<string, Shape> found = new();

        public Search(Fretboard board, HashSet<Note> allowed, HashSet<Note> required, Note bass, int maxSpan, int minStrings)
        {
            tunings = board.Tunings;
            fretCount = board.FretCount;
            this.allowed = allowed;
            this.required = required;
            this.bass = bass;
            this.maxSpan = maxSpan;
            this.minStrings = minStrings;
        }

        public IEnumerable<Shape> Run()
        {
            // Slide a window of maxSpan frets along the neck. Every fretted note of a shape lies in one
            // window, so no shape is wider than maxSpan.
            var lastWindowStart = System.Math.Max(1, fretCount - maxSpan);

            for (var windowStart = 1; windowStart <= lastWindowStart; windowStart++)
            {
                var windowEnd = System.Math.Min(windowStart + maxSpan - 1, fretCount - 1);
                var options = tunings.Select(t => OptionsFor(t, windowStart, windowEnd)).ToList();

                Choose(options, new int?[tunings.Count], 0);
            }

            return found.Values;
        }

        private List<int?> OptionsFor(Pitch tuning, int windowStart, int windowEnd)
        {
            var options = new List<int?> { null };

            for (var fret = 0; fret == 0 || fret <= windowEnd; fret++)
            {
                if (fret != 0 && fret < windowStart) continue;
                if (allowed.Contains(tuning.Note + new Interval(fret))) options.Add(fret);
            }

            return options;
        }

        private void Choose(List<List<int?>> options, int?[] frets, int stringIndex)
        {
            if (stringIndex == frets.Length)
            {
                Evaluate(frets);
                return;
            }

            foreach (var option in options[stringIndex])
            {
                frets[stringIndex] = option;
                Choose(options, frets, stringIndex + 1);
            }
        }

        private void Evaluate(int?[] frets)
        {
            var soundingStrings = Enumerable.Range(0, frets.Length).Where(i => frets[i].HasValue).ToList();

            if (soundingStrings.Count < minStrings) return;

            // No muted string between sounding ones.
            if (soundingStrings[^1] - soundingStrings[0] + 1 != soundingStrings.Count) return;

            var fretted = soundingStrings.Select(i => frets[i]!.Value).Where(f => f > 0).ToList();
            var hasOpenString = fretted.Count < soundingStrings.Count;
            var maxFret = fretted.Count == 0 ? 0 : fretted.Max();
            var span = fretted.Count == 0 ? 0 : maxFret - fretted.Min() + 1;

            if (hasOpenString && maxFret > OpenStringReach) return;

            var pitches = soundingStrings.Select(i => tunings[i] + new Interval(frets[i]!.Value)).ToList();

            if (pitches.Min().Note != bass) return;
            if (!required.IsSubsetOf(pitches.Select(p => p.Note))) return;

            // Strings are listed highest first, a diagram is read from the lowest string.
            var key = string.Join(" ", frets.Reverse().Select(f => f?.ToString() ?? "x"));

            found.TryAdd(key, new Shape((int?[])frets.Clone(), maxFret, soundingStrings.Count, span, key));
        }
    }
}
