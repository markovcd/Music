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
    
    public Intervals Intervals { get; }


    public Scale Transform(Degree degree)
    {
        var intervals = Intervals.ToList();
        AssertDegree(degree, intervals);

        var newRoot = intervals[degree - 1];
        
        return new Scale(Intervals.Create(
            intervals.Select(i => new Interval(
                Math.Modulo(i - newRoot, Note.TotalNotes)))));
    }

    private static void AssertDegree(Degree degree, IReadOnlyCollection<Interval> intervals)
    {
        if (degree > intervals.Count)
            throw new ArgumentOutOfRangeException(nameof(degree), degree, null);
    }


    
    public IReadOnlyList<Interval> GetChord(Degree degree, Chord chord)
    {
        var intervals = Intervals.ToList();
        AssertDegree(degree, intervals);
        
        // if (!chord.Degrees.All(HasDegree)) 
        //     throw new ArgumentOutOfRangeException(nameof(chord), chord, null);

        throw new NotImplementedException();
    }

    
   

}
