using System.Collections.Immutable;

namespace Domain;

public readonly record struct Note : IComparable<Note>
{
    private const string DoubleFlatSymbol = "𝄫";
    private const string DoubleSharpSymbol = "𝄪";
    private const char DefaultSharpSymbol = '#';
    private const char DefaultFlatSymbol = 'b';
    private const int FirstIndex = 0;
    private const int LastIndex = 11;
    internal const int TotalNotes = 12;
    
    private static IEnumerable<char> FlatSymbols => new[] { '♭', DefaultFlatSymbol };

    private static IEnumerable<char> SharpSymbols => new[] { '♯', DefaultSharpSymbol };

    private static IReadOnlyList<(char? name, int? index)> NoteIndexes => new (char?, int?)[]
    {
        ('C', 0), ('D', 2), ('E', 4), ('F', 5), ('G', 7), ('A', 9), ('B', 11)
    };
    
    public Note(int index)
    {
        if (index is < FirstIndex or > LastIndex)
            throw new ArgumentOutOfRangeException(nameof(index), index, null);

        Index = index;
    }

    private int Index { get; }

    private string GetName() 
    {
        var index = Index;
        var tuples = NoteIndexes;
        var note = tuples.FirstOrDefault(t => t.index == index).name;
        if (note is not null) return $"{note}";
            
        note = tuples.FirstOrDefault(t => t.index == index - 1).name;
        if (note is null) throw new InvalidOperationException();

        return $"{note}{DefaultSharpSymbol}";
    }
    
    public static Note Parse(string s)
    {
        s = s.Trim()
            .Replace(DoubleFlatSymbol, $"{DefaultFlatSymbol}{DefaultFlatSymbol}")
            .Replace(DoubleSharpSymbol, $"{DefaultSharpSymbol}{DefaultSharpSymbol}");
        
        if (s.Length < 1) throw new InvalidOperationException();
        
        var index = NoteIndexes.FirstOrDefault(t => t.name == s[0]).index;
        if (index is null) throw new InvalidOperationException();

        if (s.Length == 1) return new Note(index.Value);
            
        foreach (var c in s.Skip(1))
        {
            if (FlatSymbols.Contains(c)) index--;
            else if (SharpSymbols.Contains(c)) index++;
            else throw new InvalidOperationException();
        }

        return new Note(Math.Modulo(index!.Value, TotalNotes));
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
        return GetName();
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
