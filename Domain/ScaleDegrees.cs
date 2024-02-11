using System.Collections;

namespace Domain;

public readonly record struct ScaleDegrees : IEnumerable<ScaleDegree>
{
    public ScaleDegrees(short value)
    {
        if (value < 1) throw new ArgumentOutOfRangeException(nameof(value), value, null);
        Value = value;
    }
    
    public short Value { get; }
    
    public static ScaleDegrees Create(IEnumerable<ScaleDegree> degrees)
    {
        return new ScaleDegrees(
            (short)degrees.Distinct()
                .Select(i => (short)System.Math.Pow(2, i))
                .Sum(i => i));
    }

    public IEnumerator<ScaleDegree> GetEnumerator()
    {
        return new BitArray(new[] { (int)Value })
            .Cast<bool>()
            .Select((b, i) => (b, i))
            .Where(t => t.b)
            .Select(t => new ScaleDegree((byte)t.i))
            .OrderBy(c => c)
            .GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}