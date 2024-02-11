namespace Domain;

public static class ChordTemplates
{
    public static Degrees Triad { get; } = Degrees.Create(new Degree[] { 1, 3, 5 });
    public static Degrees Seventh { get; } = Degrees.Create(new Degree[] { 1, 3, 5, 7 });
    public static Degrees Suspended2 { get; } = Degrees.Create(new Degree[] { 1, 2, 5 });
    public static Degrees Suspended4 { get; } = Degrees.Create(new Degree[] { 1, 4, 5 });
    public static Degrees Ninth { get; } = Degrees.Create(new Degree[] { 1, 2, 3, 5, 7 });
    public static Degrees Eleventh { get; } = Degrees.Create(new Degree[] { 1, 2, 3, 4, 5, 7 });
    public static Degrees Thirteenth { get; } = Degrees.Create(new Degree[] { 1, 2, 3, 4, 5, 6, 7 });
}