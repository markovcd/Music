using System.Collections.Immutable;
namespace Domain;

public sealed record String
{
    public String(Pitch tuning, IEnumerable<Interval> intervals)
    {
        Tuning = tuning;
        
        Intervals = intervals.Distinct()
            .OrderBy(i => i)
            .ToImmutableList();

        if (Intervals.Any(i => i < 0))
            throw new ArgumentOutOfRangeException(nameof(intervals), intervals, null);
    }
    
    public Pitch Tuning { get; }

    public String Transpose(Interval interval)
    {
        return new String(Tuning + interval, Intervals);
    }

    public IReadOnlyList<Pitch> Pitches => Intervals.Select(i => Tuning + i).ToImmutableList();
    
    public IReadOnlyList<Interval> Intervals { get; }

    public bool IsPressed(Interval interval)
    {
        return Intervals.Contains(interval);
    }
    
    public String PressFret(Interval interval)
    {
        return PressFrets(new[] { interval });
    }
    
    public String PressFrets(IEnumerable<Interval> intervals)
    {
        return new String(Tuning, Intervals.Concat(intervals));
    }
    
    public String DepressFret(Interval interval)
    {
        return DepressFrets(new[] { interval });
    }
    
    public String DepressFrets(IEnumerable<Interval> intervals)
    {
        return new String(Tuning, Intervals.Except(intervals));
    }
}