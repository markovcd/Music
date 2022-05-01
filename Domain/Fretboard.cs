using System.Collections.Immutable;

namespace Domain;

public record Fretboard
{
    public Fretboard(IEnumerable<String> strings)
    {
        Strings = strings.ToImmutableList();

        if (Strings.Count == 0) 
            throw new ArgumentOutOfRangeException(nameof(strings), strings, null);
    }
    
    public IReadOnlyList<String> Strings { get; }

    public bool IsPressed(int stringIndex, Interval interval)
    {
        AssertStringIndex(stringIndex);
        return Strings[stringIndex].IsPressed(interval);
    }

    public Fretboard Transpose(Interval interval)
    {
        return new Fretboard(Strings.Select(s => s.Transpose(interval)));
    }
    
    public Fretboard PressFret(int stringIndex, Interval interval)
    {
        return ModifyString(stringIndex, s => s.PressFret(interval));
    }
    
    public Fretboard DepressFret(int stringIndex, Interval interval)
    {
        return ModifyString(stringIndex, s => s.DepressFret(interval));
    }
    
    private Fretboard ModifyString(int stringIndex, Func<String, String> action)
    {
        AssertStringIndex(stringIndex);
        
        var strings = Strings.ToList();
        strings[stringIndex] = action(strings[stringIndex]);

        return new Fretboard(strings);
    }

    private void AssertStringIndex(int stringIndex)
    {
        if (stringIndex < 0 || stringIndex >= Strings.Count)
            throw new ArgumentOutOfRangeException(nameof(stringIndex), stringIndex, null);
    }
}