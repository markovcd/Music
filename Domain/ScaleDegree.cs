namespace Domain;

public readonly record struct ScaleDegree : IComparable<ScaleDegree>
{
    public static ScaleDegree First => 1;
    public static ScaleDegree Second => 2;
    public static ScaleDegree Third => 3;
    public static ScaleDegree Fourth => 4;
    public static ScaleDegree Fifth => 5;
    public static ScaleDegree Sixth => 6;
    public static ScaleDegree Seventh => 7;


    public ScaleDegree(byte value)
    {
        if (value < 1) throw new ArgumentOutOfRangeException(nameof(value), value, null);
        Value = value;
    }
    
    public byte Value { get; }

    public int CompareTo(ScaleDegree other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static bool operator >(ScaleDegree first, ScaleDegree second)
    {
        return first.CompareTo(second) > 0;
    }
    
    public static bool operator >=(ScaleDegree first, ScaleDegree second)
    {
        return first.CompareTo(second) >= 0;
    }
    
    public static bool operator <(ScaleDegree first, ScaleDegree second)
    {
        return second > first;
    }
    
    public static bool operator <=(ScaleDegree first, ScaleDegree second)
    {
        return second >= first;
    }

    public static implicit operator int(ScaleDegree scaleDegree)
    {
        return scaleDegree.Value;
    }

    public static implicit operator ScaleDegree(byte i)
    {
        return new ScaleDegree(i);
    }
}