namespace Domain;

public readonly record struct Interval : IComparable<Interval>
{
    public static Interval Tonic => 0;
    public static Interval MinorSecond => 1;
    public static Interval MajorSecond => 2;
    public static Interval MinorThird => 3;
    public static Interval MajorThird => 4;
    public static Interval Fourth => 5;
    public static Interval AugmentedFourth => 6;
    public static Interval DiminishedFifth => 6;
    public static Interval Fifth => 7;
    public static Interval AugmentedFifth => 8;
    public static Interval MinorSixth => 8;
    public static Interval MajorSixth => 9;
    public static Interval DiminishedSeventh => 9;
    public static Interval MinorSeventh => 10;
    public static Interval MajorSeventh => 11;
    public static Interval MajorNinth => 2;
    public static Interval MinorNinth => 1;

    
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