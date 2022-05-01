namespace Domain;

public readonly record struct Octave : IComparable<Octave>
{
    public Octave(int value)
    {
        Value = value;
    }
    
    private int Value { get; }

    public int CompareTo(Octave other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static bool operator >(Octave first, Octave second)
    {
        return first.CompareTo(second) > 0;
    }
    
    public static bool operator >=(Octave first, Octave second)
    {
        return first.CompareTo(second) >= 0;
    }
    
    public static bool operator <(Octave first, Octave second)
    {
        return second > first;
    }
    
    public static bool operator <=(Octave first, Octave second)
    {
        return second >= first;
    }

    public static implicit operator int(Octave octave)
    {
        return octave.Value;
    }

    public static explicit operator Octave(int i)
    {
        return new Octave(i);
    }
    
    public static Octave operator +(Octave first, Octave second)
    {
        return new Octave(first.Value + second.Value);
    }
    
    public static Octave operator -(Octave first, Octave second)
    {
        return new Octave(first.Value - second.Value);
    }
    
    public static Octave operator -(Octave octave)
    {
        return new Octave(-octave.Value);
    }
}