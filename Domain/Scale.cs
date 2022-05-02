using System.Collections;
using System.Collections.Immutable;

namespace Domain;

public readonly record struct Scale : IComparable<Scale>
{
    public Scale(int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, null);
        Value = value;
    }
    
    public int Value { get; }

    public IReadOnlyList<Interval> GetAbsoluteIntervals()
    {
        return new BitArray(new[] { Value })
            .Cast<bool>()
            .Select((b, i) => (b, i))
            .Where(t => t.b)
            .Select(t => new Interval(t.i))
            .OrderBy(i => i)
            .ToImmutableList();
    }

    public Interval GetInterval(Degree degree)
    {
        var intervals = GetAbsoluteIntervals();
        AssertDegree(degree, intervals);
        return intervals[degree - 1];
    }
    
    public bool HasDegree(Degree degree)
    {
        return degree <= GetAbsoluteIntervals().Count;
    }

    public Scale Transform(Degree degree)
    {
        var intervals = GetAbsoluteIntervals();
        AssertDegree(degree, intervals);

        var newRoot = intervals[degree - 1];

        return FromEnumerable(
            intervals.Select(i => new Interval(
                Math.Modulo(i - newRoot, Note.TotalNotes))));
    }

    private static void AssertDegree(Degree degree, IReadOnlyCollection<Interval> intervals)
    {
        if (degree > intervals.Count)
            throw new ArgumentOutOfRangeException(nameof(degree), degree, null);
    }
    
    public IReadOnlyList<Interval> GetChord(Degree degree, Chord chord)
    {
        var intervals = GetAbsoluteIntervals();
        AssertDegree(degree, intervals);
        
        if (!chord.Degrees.All(HasDegree)) 
            throw new ArgumentOutOfRangeException(nameof(chord), chord, null);

        throw new NotImplementedException();
    }

    public Scale Normalize()
    {
        return FromEnumerable(GetAbsoluteIntervals().Select(i => i.Normalize()).Append(Interval.Zero));
    }

    public static Scale FromEnumerable(IEnumerable<Interval> intervals)
    {
        return new Scale(
            intervals.Distinct()
                .Select(i => (int)System.Math.Pow(2, i))
                .Sum());
    }
    
    public override string ToString()
    {
        return Value.ToString();
    }
    
    public int CompareTo(Scale other)
    {
        return Value.CompareTo(other.Value);
    }
    
    public static bool operator >(Scale first, Scale second)
    {
        return first.CompareTo(second) > 0;
    }
    
    public static bool operator >=(Scale first, Scale second)
    {
        return first.CompareTo(second) >= 0;
    }
    
    public static bool operator <(Scale first, Scale second)
    {
        return second > first;
    }
    
    public static bool operator <=(Scale first, Scale second)
    {
        return second >= first;
    }
    
    public static implicit operator int(Scale intervals)
    {
        return intervals.Value;
    }

    public static explicit operator Scale(int i)
    {
        return new Scale(i);
    }
    
    public static Scale operator +(Scale first, Scale second)
    {
        return new Scale(first.Value + second.Value);
    }
    
    public static Scale operator -(Scale first, Scale second)
    {
        return new Scale(first.Value - second.Value);
    }
}
