using System.Collections;
using System.Collections.Immutable;

namespace Domain;

public sealed record Intervals : IReadOnlyCollection<Interval>
{
    private readonly ImmutableHashSet<Interval> intervals;
    
    public Intervals(IEnumerable<Interval> intervals)
    {
        this.intervals = intervals.ToImmutableHashSet();
    }

    public int Count => intervals.Count;
    
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

    public bool Equals(Intervals? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Count == other.Count 
               && this.Zip(other)
                   .All(t => t.First.Equals(t.Second));
    }
    
    public override int GetHashCode()
    {
        const int prime1 = 11;
        const int prime2 = 29;
        
        unchecked 
        {
            var hash = this.Aggregate(
                prime1,
                (current, note) => current * prime2 + note.GetHashCode());

            return hash * prime2 + EqualityContract.GetHashCode();
        }
    }
}