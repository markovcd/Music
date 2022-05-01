using System.Collections;
using System.Collections.Immutable;

namespace Domain;

public record Intervals : IReadOnlyCollection<Interval>
{
    private readonly ImmutableHashSet<Interval> intervals;
    
    public Intervals(IEnumerable<Interval> intervals)
    {
        this.intervals = intervals.ToImmutableHashSet();
    }

    public Intervals Normalize()
    {
        return new Intervals(intervals.Select(i => i.Normalize()));
    }

    public static Intervals FromInt(int identifier)
    {
        if (identifier < 0) throw new ArgumentOutOfRangeException(nameof(identifier), identifier, null);
        
        return new Intervals(
            new BitArray(new [] { identifier })
                .Cast<bool>()
                .Select((b, i) => (b, i))
                .Where(t => t.b)
                .Select(t => new Interval(t.i)));
    }

    public IEnumerator<Interval> GetEnumerator()
    {
        return intervals.OrderBy(i => i).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => intervals.Count;
}