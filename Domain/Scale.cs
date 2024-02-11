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
        return new Scale(Intervals.Transform(degree));
    }
    


    
    
    
}