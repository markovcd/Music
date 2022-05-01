using System.Collections.Immutable;
namespace Domain;

public sealed record String
{
    public String(Pitch tuning, IEnumerable<Interval> intervals)
    {
        Tuning = tuning;
        Intervals = intervals.ToImmutableHashSet().OrderBy(i => i).ToImmutableList();

        foreach (var interval in Intervals) AssertInterval(interval);
    }
    
    public Pitch Tuning { get; }

    public IReadOnlyList<Pitch> Pitches => Intervals.Select(i => Tuning + i).ToImmutableList();
    
    public IReadOnlyList<Interval> Intervals { get; }

    public bool IsPressed(Interval interval)
    {
        AssertInterval(interval);
        return Intervals.Contains(interval);
    }
    
    public String PressFret(Interval interval)
    {
        AssertInterval(interval);
        return new String(Tuning, Intervals.Append(interval));
    }
    
    public String DepressFret(Interval interval)
    {
        AssertInterval(interval);
        return new String(Tuning, Intervals.Except(new[] { interval }));
    }

    private static void AssertInterval(Interval interval)
    {
        if (interval < 0) throw new ArgumentOutOfRangeException(nameof(interval), interval, null);
    }
}