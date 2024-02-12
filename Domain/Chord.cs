using System.Collections.Immutable;

namespace Domain;

public readonly record struct Chord(Degrees Degrees, Intervals Intervals)
{
    public Interval GetInterval(Degree degree)
    {
        var index = Degrees.Select((d, i) => (d, i)).Single(t => t.d == degree).i;
        var intervals = Intervals.ToImmutableArray();

        if (index > intervals.Length)
            throw new ArgumentOutOfRangeException(nameof(degree), degree, null);

        return intervals[index];
    }

    public bool Contains(IEnumerable<Degree> degrees)
    {
        var local = this;
        return degrees.All(d => local.Degrees.Contains(d));
    }
    
    public TriadType TriadType
    {
        get
        {
            if (!Contains(ChordTemplates.Triad)) return TriadType.None;
            
            var third = GetInterval(Degree.Third);
            var fifth = GetInterval(Degree.Fifth);
            
            if ((third, fifth) == (Interval.MinorThird, Interval.Fifth)) return TriadType.Minor;
            if ((third, fifth) == (Interval.MajorThird, Interval.Fifth)) return TriadType.Major;
            if ((third, fifth) == (Interval.MinorThird, Interval.DiminishedFifth)) return TriadType.Diminished;
            if ((third, fifth) == (Interval.MajorThird, Interval.AugmentedFifth)) return TriadType.Augmented;

            return TriadType.None;
        }
    }

    public SeventhType SeventhType
    {
        get
        {
            if (!Contains(ChordTemplates.Seventh)) return SeventhType.None;
            var triadType = TriadType;
            var seventh = GetInterval(Degree.Seventh);
            
            if ((triadType, seventh) == (TriadType.Minor, Interval.MinorSeventh)) return SeventhType.Minor;
            if ((triadType, seventh) == (TriadType.Major, Interval.MajorSeventh)) return SeventhType.Major;
            if ((triadType, seventh) == (TriadType.Minor, Interval.MajorSeventh)) return SeventhType.MinorMajor;
            if ((triadType, seventh) == (TriadType.Major, Interval.MinorSeventh)) return SeventhType.Dominant;
            if ((triadType, seventh) == (TriadType.Diminished, Interval.MinorSeventh)) return SeventhType.HalfDiminished;
            if ((triadType, seventh) == (TriadType.Diminished, Interval.DiminishedSeventh)) return SeventhType.Diminished;
            if ((triadType, seventh) == (TriadType.Diminished, Interval.MajorSeventh)) return SeventhType.DiminishedMajor;
            if ((triadType, seventh) == (TriadType.Augmented, Interval.MinorSeventh)) return SeventhType.Augmented;
            if ((triadType, seventh) == (TriadType.Augmented, Interval.MajorSeventh)) return SeventhType.AugmentedMajor;
            
            return SeventhType.None;
        }
    }
}