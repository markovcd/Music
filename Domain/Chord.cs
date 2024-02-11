using System.Collections.Immutable;

namespace Domain;

public readonly record struct Chord(ScaleDegrees ScaleDegrees, Intervals Intervals)
{
    public Interval GetInterval(ScaleDegree scaleDegree)
    {
        var index = ScaleDegrees.Select((d, i) => (d, i)).Single(t => t.d == scaleDegree).i;
        var intervals = Intervals.ToImmutableArray();

        if (index > intervals.Length)
            throw new ArgumentOutOfRangeException(nameof(scaleDegree), scaleDegree, null);

        return intervals[index];
    }
}