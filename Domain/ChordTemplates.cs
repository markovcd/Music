namespace Domain;

public static class ChordTemplates
{
    public static Degrees Triad { get; } 
        = Degrees.Create(new [] { Degree.First, Degree.Third, Degree.Fifth });
    
    public static Degrees Seventh { get; } 
        = Degrees.Create(new [] { Degree.First, Degree.Third, Degree.Fifth, Degree.Seventh, });
    
    public static Degrees Suspended2 { get; } 
        = Degrees.Create(new [] { Degree.First, Degree.Second, Degree.Fifth });
    
    public static Degrees Suspended4 { get; } 
        = Degrees.Create(new [] { Degree.First, Degree.Fourth, Degree.Fifth });
    
    public static Degrees Ninth { get; }
        = Degrees.Create(new [] { Degree.First, Degree.Third, Degree.Fifth, Degree.Seventh, Degree.Ninth });
    
    public static Degrees Eleventh { get; }
        = Degrees.Create(new [] { Degree.First, Degree.Third, Degree.Fifth, Degree.Seventh, Degree.Ninth, Degree.Eleventh });
    
    public static Degrees Thirteenth { get; } 
        = Degrees.Create(new [] { Degree.First, Degree.Third, Degree.Fifth, Degree.Seventh, Degree.Ninth, Degree.Eleventh, Degree.Thirteenth });
}