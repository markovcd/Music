using System.Collections;

namespace Domain;

public readonly record struct Degrees : IEnumerable<Degree>
{
    public Degrees(int value)
    {
        if (value < 1) throw new ArgumentOutOfRangeException(nameof(value), value, null);
        Value = value;
    }
    
    public int Value { get; }
    
    public static Degrees Create(IEnumerable<Degree> degrees)
    {
        return new Degrees(
            degrees.Distinct()
                .Select(i => (int)System.Math.Pow(2, i))
                .Sum());
    }

    public IEnumerator<Degree> GetEnumerator()
    {
        return new BitArray(new[] { Value })
            .Cast<bool>()
            .Select((b, i) => (b, i))
            .Where(t => t.b)
            .Select(t => new Degree(t.i))
            .OrderBy(c => c)
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}