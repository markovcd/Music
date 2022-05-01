using System.Collections;
using System.Collections.Immutable;

namespace Domain;

public readonly record struct Chord
{
    public Chord(int value)
    {
        if (value < 1) throw new ArgumentOutOfRangeException(nameof(value), value, null);
        Value = value;
    }
    
    public int Value { get; }
    
    public IReadOnlyList<Degree> Degrees => new BitArray(new[] { Value })
        .Cast<bool>()
        .Select((b, i) => (b, i))
        .Where(t => t.b)
        .Select(t => new Degree(t.i))
        .ToImmutableList();
    
    public static Chord FromEnumerable(IEnumerable<Degree> degrees)
    {
        return new Chord(
            degrees.Distinct()
                .Select(i => (int)System.Math.Pow(2, i))
                .Sum());
    }
}