namespace Domain;

public readonly record struct Degree : IComparable<Degree>
{
    public Degree(int value)
    {
        if (value < 1) throw new ArgumentOutOfRangeException(nameof(value), value, null);
        Value = value;
    }
    
    public int Value { get; }

    public int CompareTo(Degree other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static bool operator >(Degree first, Degree second)
    {
        return first.CompareTo(second) > 0;
    }
    
    public static bool operator >=(Degree first, Degree second)
    {
        return first.CompareTo(second) >= 0;
    }
    
    public static bool operator <(Degree first, Degree second)
    {
        return second > first;
    }
    
    public static bool operator <=(Degree first, Degree second)
    {
        return second >= first;
    }

    public static implicit operator int(Degree octave)
    {
        return octave.Value;
    }

    public static explicit operator Degree(int i)
    {
        return new Degree(i);
    }
}