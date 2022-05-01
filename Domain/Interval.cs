namespace Domain;

public readonly record struct Interval : IComparable<Interval>
{
    public Interval(int value)
    {
        Value = value;
    }
    
    private int Value { get; }
    
    public int CompareTo(Interval other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
    
    public static bool operator >(Interval first, Interval second)
    {
        return first.CompareTo(second) > 0;
    }
    
    public static bool operator >=(Interval first, Interval second)
    {
        return first.CompareTo(second) >= 0;
    }
    
    public static bool operator <(Interval first, Interval second)
    {
        return second > first;
    }
    
    public static bool operator <=(Interval first, Interval second)
    {
        return second >= first;
    }

    public static implicit operator int(Interval interval)
    {
        return interval.Value;
    }
    
    public static explicit operator Interval(int i)
    {
        return new Interval(i);
    }

    public static Interval operator +(Interval first, Interval second)
    {
        return new Interval(first.Value + second.Value);
    }
    
    public static Interval operator -(Interval first, Interval second)
    {
        return new Interval(first.Value - second.Value);
    }
    
    public static Interval operator -(Interval interval)
    {
        return new Interval(-interval.Value);
    }
}