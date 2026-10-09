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
    /// The name of the chord stacked on <paramref name="chordRoot"/>, or null if its notes
    /// do not form a chord known to <see cref="ChordQualities"/> with the chord root as the root.
    /// </summary>
    public ChordName? GetChordName(Note scaleRoot, Degree chordRoot, IReadOnlyList<Degree> template)
    {
        var notes = GetChordNotes(scaleRoot, chordRoot, template);

        return ChordNamer.Detect(notes, notes[0])
            .Where(name => name.Bass is null)
            .Select(name => (ChordName?)name)
            .FirstOrDefault();
    }

    /// <summary>
    /// The names of the chords stacked on every degree of the scale, in degree order,
    /// for example Am7, Bm7b5, Cmaj7, Dm7, Em7, Fmaj7, G7 for the seventh chords of A minor.
    /// A chord that is not a known kind of chord is null.
    /// </summary>
    public IReadOnlyList<ChordName?> GetChordNames(Note scaleRoot, IReadOnlyList<Degree> template)
    {
        var local = this;

        return Enumerable.Range(1, Intervals.Count())
            .Select(i => local.GetChordName(scaleRoot, new Degree((byte)i), template))
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