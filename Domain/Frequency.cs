using System.Globalization;

namespace Domain;

public readonly record struct Frequency : IComparable<Frequency>
{
    public Frequency(double hertz)
    {
        if (double.IsNaN(hertz) || hertz <= 0)
            throw new ArgumentOutOfRangeException(nameof(hertz), hertz, null);
        
        Hertz = hertz;
    }
    
    private double Hertz { get; }

    public int CompareTo(Frequency other)
    {
        return Hertz.CompareTo(other.Hertz);
    }
    
    public override string ToString()
    {
        return Hertz.ToString(CultureInfo.CurrentCulture);
    }

    public static bool operator >(Frequency first, Frequency second)
    {
        return first.CompareTo(second) > 0;
    }
    
    public static bool operator >=(Frequency first, Frequency second)
    {
        return first.CompareTo(second) >= 0;
    }
    
    public static bool operator <(Frequency first, Frequency second)
    {
        return second > first;
    }
    
    public static bool operator <=(Frequency first, Frequency second)
    {
        return second >= first;
    }

    public static implicit operator double(Frequency frequency)
    {
        return frequency.Hertz;
    }

    public static explicit operator Frequency(double d)
    {
        return new Frequency(d);
    }
    
    public static Frequency operator +(Frequency first, Frequency second)
    {
        return new Frequency(first.Hertz + second.Hertz);
    }
    
    public static Frequency operator -(Frequency first, Frequency second)
    {
        return new Frequency(first.Hertz - second.Hertz);
    }
}