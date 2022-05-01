using System.Collections;
using System.Collections.Immutable;

namespace Domain;

public readonly record struct Intervals : IComparable<Intervals>
{
    public Intervals(int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, null);
        Value = value;
    }
    
    public int Value { get; }

    public IReadOnlyList<Interval> Values => new BitArray(new[] { Value })
        .Cast<bool>()
        .Select((b, i) => (b, i))
        .Where(t => t.b)
        .Select(t => new Interval(t.i))
        .ToImmutableList();

    public Intervals Normalize()
    {
        return FromEnumerable(Values.Select(i => i.Normalize()));
    }

    public static Intervals FromEnumerable(IEnumerable<Interval> intervals)
    {
        return new Intervals(
            intervals.ToImmutableHashSet()
                .Select(i => (int)System.Math.Pow(2, i))
                .Sum());
    }
    
    public override string ToString()
    {
        return Value.ToString();
    }
    
    public int CompareTo(Intervals other)
    {
        return Value.CompareTo(other.Value);
    }
    
    public static bool operator >(Intervals first, Intervals second)
    {
        return first.CompareTo(second) > 0;
    }
    
    public static bool operator >=(Intervals first, Intervals second)
    {
        return first.CompareTo(second) >= 0;
    }
    
    public static bool operator <(Intervals first, Intervals second)
    {
        return second > first;
    }
    
    public static bool operator <=(Intervals first, Intervals second)
    {
        return second >= first;
    }
    
    public static implicit operator int(Intervals intervals)
    {
        return intervals.Value;
    }

    public static explicit operator Intervals(int i)
    {
        return new Intervals(i);
    }
    
    public static Intervals operator +(Intervals first, Intervals second)
    {
        return new Intervals(first.Value + second.Value);
    }
    
    public static Intervals operator -(Intervals first, Intervals second)
    {
        return new Intervals(first.Value - second.Value);
    }
}