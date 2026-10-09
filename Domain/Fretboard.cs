using System.Collections.Immutable;

namespace Domain;

/// <summary>
/// An immutable fretboard: a set of strings, each with a tuning and a set of pressed frets.
/// Frets are numbered from 0 (the open string), so pressing fret 0 means the open string is played.
/// When several frets are pressed on one string, the highest one sounds.
/// A string with no pressed fret is muted.
/// </summary>
public sealed class Fretboard
{
    public const int DefaultFretCount = 24;

    /// <summary>Standard guitar tuning, from the highest string (E4) to the lowest (E2).</summary>
    public static IReadOnlyList<Pitch> StandardTuning { get; } = new[]
    {
        new Pitch(new Octave(4), Note.Parse("E")),
        new Pitch(new Octave(3), Note.Parse("B")),
        new Pitch(new Octave(3), Note.Parse("G")),
        new Pitch(new Octave(3), Note.Parse("D")),
        new Pitch(new Octave(2), Note.Parse("A")),
        new Pitch(new Octave(2), Note.Parse("E")),
    };

    private readonly ImmutableArray<Pitch> tunings;
    private readonly ImmutableArray<ImmutableSortedSet<Interval>> pressed;

    public Fretboard(IEnumerable<Pitch> tunings, int fretCount = DefaultFretCount)
        : this(
            tunings.ToImmutableArray(),
            fretCount,
            default)
    {
    }

    private Fretboard(
        ImmutableArray<Pitch> tunings,
        int fretCount,
        ImmutableArray<ImmutableSortedSet<Interval>> pressed)
    {
        if (tunings.IsDefaultOrEmpty)
            throw new ArgumentException("A fretboard needs at least one string.", nameof(tunings));

        if (fretCount < 1)
            throw new ArgumentOutOfRangeException(nameof(fretCount), fretCount, null);

        this.tunings = tunings;
        FretCount = fretCount;
        this.pressed = pressed.IsDefault
            ? tunings.Select(_ => ImmutableSortedSet<Interval>.Empty).ToImmutableArray()
            : pressed;
    }

    public static Fretboard Standard(int fretCount = DefaultFretCount)
    {
        return new Fretboard(StandardTuning, fretCount);
    }

    public IReadOnlyList<Pitch> Tunings => tunings;

    public int StringCount => tunings.Length;

    /// <summary>Number of frets per string, counting the open string (fret 0).</summary>
    public int FretCount { get; }

    public bool IsPressed(int stringIndex, Interval interval)
    {
        AssertString(stringIndex);
        return pressed[stringIndex].Contains(interval);
    }

    public Fretboard PressFret(int stringIndex, Interval interval)
    {
        AssertString(stringIndex);
        AssertFret(interval, nameof(interval));
        return ModifyString(stringIndex, frets => frets.Add(interval));
    }

    public Fretboard DepressFret(int stringIndex, Interval interval)
    {
        AssertString(stringIndex);
        return ModifyString(stringIndex, frets => frets.Remove(interval));
    }

    /// <summary>
    /// Moves every pressed fret by the given interval (a number of frets, negative moves towards the nut).
    /// </summary>
    public Fretboard Transpose(Interval interval)
    {
        var transposed = pressed
            .Select(frets => frets.Select(f => f + interval).ToImmutableSortedSet())
            .ToImmutableArray();

        if (transposed.SelectMany(frets => frets).Any(f => !IsValidFret(f)))
            throw new ArgumentOutOfRangeException(nameof(interval), interval, null);

        return new Fretboard(tunings, FretCount, transposed);
    }

    /// <summary>The fret that sounds on the string (the highest pressed one), or null if the string is muted.</summary>
    public Interval? GetFret(int stringIndex)
    {
        AssertString(stringIndex);
        var frets = pressed[stringIndex];
        return frets.IsEmpty ? null : frets.Max;
    }

    /// <summary>The pitch that sounds on the string, or null if the string is muted.</summary>
    public Pitch? GetPitch(int stringIndex)
    {
        var fret = GetFret(stringIndex);
        return fret.HasValue ? tunings[stringIndex] + fret.Value : null;
    }

    /// <summary>
    /// The sounding fret of every string as chord diagrams are usually written, with "x" for a muted string,
    /// starting from the last string, which is the lowest one on a board that lists the highest string first
    /// (like <see cref="Standard"/>). For example "x 3 2 0 1 0" for an open C chord.
    /// </summary>
    public string Diagram =>
        string.Join(" ", Enumerable.Range(0, StringCount).Reverse()
            .Select(GetFret)
            .Select(f => f.HasValue ? ((int)f.Value).ToString() : "x"));

    /// <summary>The pitches that sound, in string order. Muted strings are skipped.</summary>
    public IEnumerable<Pitch> GetPitches()
    {
        return Enumerable.Range(0, StringCount)
            .Select(GetPitch)
            .Where(p => p.HasValue)
            .Select(p => p!.Value);
    }

    /// <summary>
    /// The chord names that fit the pitches that sound, best first. See <see cref="ChordNamer"/>.
    /// Pass the note names of a key to spell the chords the way that key does.
    /// </summary>
    public IReadOnlyList<ChordName> GetChordNames(NoteNames? names = null)
    {
        return ChordNamer.Detect(GetPitches(), names);
    }

    private Fretboard ModifyString(
        int stringIndex,
        Func<ImmutableSortedSet<Interval>, ImmutableSortedSet<Interval>> action)
    {
        var current = pressed[stringIndex];
        var modified = action(current);

        return ReferenceEquals(current, modified)
            ? this
            : new Fretboard(tunings, FretCount, pressed.SetItem(stringIndex, modified));
    }

    private bool IsValidFret(Interval interval)
    {
        return interval >= Interval.Tonic && interval < FretCount;
    }

    private void AssertString(int stringIndex)
    {
        if (stringIndex < 0 || stringIndex >= StringCount)
            throw new ArgumentOutOfRangeException(nameof(stringIndex), stringIndex, null);
    }

    private void AssertFret(Interval interval, string paramName)
    {
        if (!IsValidFret(interval))
            throw new ArgumentOutOfRangeException(paramName, interval, null);
    }
}
