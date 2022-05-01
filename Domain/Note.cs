using System.Collections.Immutable;

namespace Domain;

public readonly record struct Note : IComparable<Note>
{
    private const int FirstIndex = 0;
    private const int LastIndex = 11;

    internal const int TotalNotes = 12;
    
    private static readonly IReadOnlyList<string> Names = new[]
    {
        "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B",
    }.ToImmutableList();
    
    public Note(int index)
    {
        if (index is < FirstIndex or > LastIndex)
            throw new ArgumentOutOfRangeException(nameof(index), index, null);

        Index = index;
    }

    private int Index { get; }

    private string Name => Names[Index];
    
    public static Note Parse(string s)
    {
        s = s.Trim().ToUpperInvariant();
        if (s.Length is > 2 or < 1) throw new InvalidOperationException();

        var index = Names.Select((n, i) => (n, i)).Single(t => t.n == s).i;
        return new Note(index);
    }
    
    public Note Transpose(Interval interval)
    {
        return new Note(Math.Modulo(Index + interval, TotalNotes));
    }

    public int CompareTo(Note other)
    {
        return Index.CompareTo(other.Index);
    }
    
    public override string ToString()
    {
        return Name;
    }

    public static bool operator >(Note first, Note second)
    {
        return first.CompareTo(second) > 0;
    }
    
    public static bool operator >=(Note first, Note second)
    {
        return first.CompareTo(second) >= 0;
    }
    
    public static bool operator <(Note first, Note second)
    {
        return second > first;
    }
    
    public static bool operator <=(Note first, Note second)
    {
        return second >= first;
    }
    
    public static implicit operator int(Note other)
    {
        return other.Index;
    }

    public static explicit operator Note(int other)
    {
        return new Note(other);
    }
    
    public static Note operator +(Note note, Interval interval)
    {
        return note.Transpose(interval);
    }
    
    public static Note operator -(Note note, Interval interval)
    {
        return note.Transpose(-interval);
    }
}