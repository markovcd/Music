namespace Domain;

public readonly record struct Interval : IComparable<Interval>
{
    public static Interval Zero => new (0);
    
    public Interval(int value)
    {
        Value = value;
    }
    
    private int Value { get; }

    public Interval Normalize()
    {
        return new Interval(Math.Modulo(Value, Note.TotalNotes));
    }
    
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
    
    public static implicit operator Interval(int i)
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