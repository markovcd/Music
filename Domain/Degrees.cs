using System.Collections;

namespace Domain;

public readonly record struct Degrees : IEnumerable<Degree>
{
    public Degrees(short value)
    {
        if (value < 1) throw new ArgumentOutOfRangeException(nameof(value), value, null);
        Value = value;
    }
    
    public short Value { get; }
    
    public static Degrees Create(IEnumerable<Degree> degrees)
    {
        return new Degrees(
            (short)degrees.Distinct()
                .Select(i => (short)System.Math.Pow(2, i))
                .Sum(i => i));
    }

    public IEnumerator<Degree> GetEnumerator()
    {
        return new BitArray(new[] { (int)Value })
            .Cast<bool>()
            .Select((b, i) => (b, i))
            .Where(t => t.b)
            .Select(t => new Degree((byte)t.i))
            .OrderBy(c => c)
            .GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}