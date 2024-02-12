namespace Domain;

public readonly record struct Degree : IComparable<Degree>
{
    public static Degree First => 1;
    public static Degree Second => 2;
    public static Degree Third => 3;
    public static Degree Fourth => 4;
    public static Degree Fifth => 5;
    public static Degree Sixth => 6;
    public static Degree Seventh => 7;
    public static Degree Ninth => 2;
    public static Degree Eleventh => 4;
    public static Degree Thirteenth => 6;
    
    public Degree(byte value)
    {
        if (value < 1) throw new ArgumentOutOfRangeException(nameof(value), value, null);
        Value = value;
    }
    
    public byte Value { get; }

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

    public static implicit operator int(Degree degree)
    {
        return degree.Value;
    }

    public static implicit operator Degree(byte i)
    {
        return new Degree(i);
    }
}