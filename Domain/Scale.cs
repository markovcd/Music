using System.Collections;
using System.Collections.Immutable;

namespace Domain;

public readonly record struct Scale
{
    public Scale(Intervals intervals)
    {
        if (intervals.Normalize() != intervals) 
            throw new ArgumentOutOfRangeException(nameof(intervals));
        
        Intervals = intervals;
    }

    public static Scale Create(IEnumerable<Interval> intervals)
    {
        return new Scale(Intervals.Create(intervals));
    }
    
    public Intervals Intervals { get; }
    
    public Scale Transform(Degree degree)
    {
        AssertDegree(degree);
    
        var intervals = Intervals.ToList();
        var newRoot = intervals[degree - 1];
        
        return new Scale(Intervals.Create(
            intervals.Select(i => new Interval(
                Math.Modulo(i - newRoot, Note.TotalNotes)))));
    }
    
    private void AssertDegree(Degree degree)
    {
        if (degree > Intervals.Count())
            throw new ArgumentOutOfRangeException(nameof(degree), degree, null);
    }
    
    public bool HasDegree(Degree degree)
    {
        return degree <= Intervals.Count();
    }
    
    /// <summary>
    /// The notes of the chord stacked on <paramref name="chordRoot"/>, in the order of <paramref name="template"/>,
    /// when the scale starts on <paramref name="scaleRoot"/>. A template degree counts from the chord root,
    /// so a triad on the second degree uses the scale's second, fourth and sixth degrees.
    /// </summary>
    public IReadOnlyList<Note> GetChordNotes(Note scaleRoot, Degree chordRoot, IReadOnlyList<Degree> template)
    {
        AssertDegree(chordRoot);

        if (template.Count == 0)
            throw new ArgumentException("A chord needs at least one degree.", nameof(template));

        if (!template.All(HasDegree))
            throw new ArgumentOutOfRangeException(nameof(template), template, null);

        var local = this;
        var count = Intervals.Count();

        return template
            .Select(d => local.GetInterval(new Degree((byte)((chordRoot - 1 + d - 1) % count + 1))))
            .Select(i => scaleRoot + i)
            .ToList();
    }

    /// <summary>
    /// The letter names of the scale's notes when it starts on <paramref name="root"/>.
    /// A seven-note scale uses each letter once, so Db major is Db Eb F Gb Ab Bb C and C# minor is C# D# E F# G# A B.
    /// Other scales use the usual name of each note. <paramref name="rootName"/> picks between spellings of the root
    /// (C# or Db); by default it is the usual one for the scale, which is Db for major and C# for minor.
    /// Where a letter name would need a double sharp or flat, the usual name of the note is used instead.
    /// </summary>
    public NoteNames GetNoteNames(Note root, string? rootName = null)
    {
        var minorThird = Intervals.HasInterval(3) && !Intervals.HasInterval(4);

        rootName ??= NoteSpelling.SpellRoot(root, minorThird);

        if (!IsNameOf(rootName, root))
            throw new ArgumentException($"\"{rootName}\" is not a name for the note {root}.", nameof(rootName));

        var intervals = Intervals.ToList();
        var letterPerNote = intervals.Count == 7;

        return new NoteNames(intervals.Select((interval, index) =>
        {
            var note = root + interval;

            var name = index == 0 ? rootName
                : letterPerNote ? NoteSpelling.SpellTone(rootName, index, note, minorThird)
                : NoteSpelling.SpellRoot(note, minorThird);

            return KeyValuePair.Create(note, name);
        }));
    }

    private static bool IsNameOf(string name, Note note)
    {
        try
        {
            return Note.Parse(name) == note;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>
    /// The name of the chord stacked on <paramref name="chordRoot"/>, or null if its notes
    /// do not form a chord known to <see cref="ChordQualities"/> with the chord root as the root.
    /// Notes are spelled the way the scale spells them (see <see cref="GetNoteNames"/>).
    /// </summary>
    public ChordName? GetChordName(Note scaleRoot, Degree chordRoot, IReadOnlyList<Degree> template, string? scaleRootName = null)
    {
        return GetChordName(scaleRoot, chordRoot, template, GetNoteNames(scaleRoot, scaleRootName));
    }

    private ChordName? GetChordName(Note scaleRoot, Degree chordRoot, IReadOnlyList<Degree> template, NoteNames names)
    {
        var notes = GetChordNotes(scaleRoot, chordRoot, template);

        return ChordNamer.Detect(notes, notes[0], names)
            .Where(name => name.Bass is null)
            .Select(name => (ChordName?)name)
            .FirstOrDefault();
    }

    /// <summary>
    /// The names of the chords stacked on every degree of the scale, in degree order,
    /// for example Am7, Bm7b5, Cmaj7, Dm7, Em7, Fmaj7, G7 for the seventh chords of A minor.
    /// A chord that is not a known kind of chord is null.
    /// </summary>
    public IReadOnlyList<ChordName?> GetChordNames(Note scaleRoot, IReadOnlyList<Degree> template, string? scaleRootName = null)
    {
        var local = this;
        var names = GetNoteNames(scaleRoot, scaleRootName);

        return Enumerable.Range(1, Intervals.Count())
            .Select(i => local.GetChordName(scaleRoot, new Degree((byte)i), template, names))
            .ToList();
    }

    public Interval GetInterval(Degree degree)
    {
        var intervals = Intervals.ToImmutableArray();
    
        if (degree > intervals.Length)
            throw new ArgumentOutOfRangeException(nameof(degree), degree, null);
    
        return intervals[degree - 1];
    }
}