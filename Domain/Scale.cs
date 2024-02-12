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
    
    public Chord GetChord(Degree root, Degrees template)
    {
        if (!template.All(HasDegree)) 
            throw new ArgumentOutOfRangeException(nameof(template), template, null);

        var transformed = Transform(root);

        return new Chord(template, Intervals.Create(template.Select(d => transformed.GetInterval(d))));
    }

    public IEnumerable<Chord> GetChords(Degrees template)
    {
        var count = Intervals.Count();
        var local = this;
        return Enumerable.Range(1, count).Select(i => local.GetChord((byte)i, template));
    }
    
    public Interval GetInterval(Degree degree)
    {
        var intervals = Intervals.ToImmutableArray();
    
        if (degree > intervals.Length)
            throw new ArgumentOutOfRangeException(nameof(degree), degree, null);
    
        return intervals[degree - 1];
    }
}