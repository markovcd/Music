using System.Collections.Immutable;

namespace Domain;

public enum TriadType
{
    None,
    Minor,
    Major,
    Diminished,
    Augmented
}

public enum SeventhType
{
    None,
    Minor,
    Major,
    MinorMajor,
    Dominant,
    HalfDiminished,
    Diminished,
    DiminishedMajor,
    Augmented,
    AugmentedSeventh
}

public readonly record struct Chord(Degree Root, Degrees Degrees, Intervals Intervals)
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

    
    public bool ContainsSeventh => Contains(ChordTemplates.Seventh);
    
    public bool IsTriad => Degrees == ChordTemplates.Triad;
    
    public bool IsSeventh => Degrees == ChordTemplates.Seventh;

    public TriadType TriadType
    {
        get
        {
            if (!Contains(ChordTemplates.Triad)) return TriadType.None;
            
            int third = GetInterval(3);
            int fifth = GetInterval(5);
            
            return (third, fifth) switch
            {
                (3, 7) => TriadType.Minor,
                (4, 7) => TriadType.Major,
                (3, 6) => TriadType.Diminished,
                (4, 8) => TriadType.Augmented,
                _ => TriadType.None
            };
        }
    }

    public SeventhType SeventhType
    {
        get
        {
            if (!Contains(ChordTemplates.Seventh)) return SeventhType.None;
            var triadType = TriadType;
            var seventh = GetInterval(7);
            
            return (TriadType, seventh) switch
            {
                (TriadType.Minor, 7) => SeventhType.Minor,
                (TriadType.Minor, 7) => TriadType.Major,
                (TriadType.Minor, 6) => TriadType.Diminished,
                (TriadType.Minor, 8) => TriadType.Augmented,
                _ => TriadType.None
            };
        }
    }
}