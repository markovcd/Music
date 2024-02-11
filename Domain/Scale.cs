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


    public Scale Transform(ScaleDegree scaleDegree)
    {
        AssertDegree(scaleDegree);
    
        var intervals = Intervals.ToList();
        var newRoot = intervals[scaleDegree - 1];
        
        return new Scale(Intervals.Create(
            intervals.Select(i => new Interval(
                Math.Modulo(i - newRoot, Note.TotalNotes)))));
    }
    
    private void AssertDegree(ScaleDegree scaleDegree)
    {
        if (scaleDegree > Intervals.Count())
            throw new ArgumentOutOfRangeException(nameof(scaleDegree), scaleDegree, null);
    }
    
    public bool HasDegree(ScaleDegree scaleDegree)
    {
        return scaleDegree <= Intervals.Count();
    }
    
    public Chord GetChord(ScaleDegree root, ScaleDegrees template)
    {
        if (!template.All(HasDegree)) 
            throw new ArgumentOutOfRangeException(nameof(template), template, null);

        var transformed = Transform(root);

        return new Chord(template, Intervals.Create(template.Select(d => transformed.GetInterval(d))));
    }
    
    public Interval GetInterval(ScaleDegree scaleDegree)
    {
        var intervals = Intervals.ToImmutableArray();
    
        if (scaleDegree > intervals.Length)
            throw new ArgumentOutOfRangeException(nameof(scaleDegree), scaleDegree, null);
    
        return intervals[scaleDegree - 1];
    }
}