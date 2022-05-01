namespace Domain;

public readonly record struct Pitch(Octave Octave, Note Note) : IComparable<Pitch>
{
    private static (Pitch Pitch, Frequency Frequency) Reference => 
        (new Pitch(new Octave(4), Note.Parse("A")), new Frequency(440));

    public int Index => Octave * Note.TotalNotes + Note;
    
    public Frequency Frequency => CalculateRelativeFrequency(
        Reference.Frequency, 
        Reference.Pitch.Index,
        Index);

    public Pitch Transpose(Interval interval)
    {
        var transposedNote = Note + interval;
        var transposedOctave = Octave + new Octave ((interval - transposedNote + Note) / Note.TotalNotes);
        
        return new Pitch(transposedOctave, transposedNote);
    }
    
    public static Pitch FromIndex(int index)
    {
        int Modulo(int x, int m) => (x % m + m) % m;
        
        var octave = new Octave(index / Note.TotalNotes);
        var note = new Note(Modulo(index, Note.TotalNotes));
        
        return new Pitch(octave, note);
    }

    public static Pitch FromFrequency(Frequency frequency)
    {
        var index = GetIndexFromFrequency(Reference.Frequency, Reference.Pitch.Index, frequency);
        return FromIndex(index);
    }

    private static Frequency CalculateRelativeFrequency(Frequency referenceFrequency, int referenceIndex, int index)
    {
        return new Frequency(
            referenceFrequency * System.Math.Pow(2, (index - referenceIndex) / (double)Note.TotalNotes));
    }
    
    private static int GetIndexFromFrequency(Frequency referenceFrequency, int referenceIndex, Frequency frequency)
    {
        return (int)System.Math.Round(
            System.Math.Log2(frequency / referenceFrequency) * Note.TotalNotes) + referenceIndex;
    }

    public int CompareTo(Pitch other)
    {
        return Index.CompareTo(other.Index);
    }
    
    public override string ToString()
    {
        return $"{Note}{Octave}";
    }
    
    public static bool operator >(Pitch first, Pitch second)
    {
        return first.CompareTo(second) > 0;
    }
    
    public static bool operator >=(Pitch first, Pitch second)
    {
        return first.CompareTo(second) >= 0;
    }
    
    public static bool operator <(Pitch first, Pitch second)
    {
        return second > first;
    }
    
    public static bool operator <=(Pitch first, Pitch second)
    {
        return second >= first;
    }
    
    public static explicit operator Frequency(Pitch pitch)
    {
        return pitch.Frequency;
    }
    
    public static explicit operator Pitch(Frequency frequency)
    {
        return FromFrequency(frequency);
    }
    
    public static Pitch operator +(Pitch pitch, Interval interval)
    {
        return pitch.Transpose(interval);
    }
    
    public static Pitch operator -(Pitch pitch, Interval interval)
    {
        return pitch.Transpose(-interval);
    }
}