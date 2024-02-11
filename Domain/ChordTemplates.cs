namespace Domain;

public static class ChordTemplates
{
    public static ScaleDegrees Triad { get; } = ScaleDegrees.Create(new ScaleDegree[] { 1, 3, 5 });
    public static ScaleDegrees Seventh { get; } = ScaleDegrees.Create(new ScaleDegree[] { 1, 3, 5, 7 });
    public static ScaleDegrees Suspended2 { get; } = ScaleDegrees.Create(new ScaleDegree[] { 1, 2, 5 });
    public static ScaleDegrees Suspended4 { get; } = ScaleDegrees.Create(new ScaleDegree[] { 1, 4, 5 });
    public static ScaleDegrees Ninth { get; } = ScaleDegrees.Create(new ScaleDegree[] { 1, 2, 3, 5, 7 });
    public static ScaleDegrees Eleventh { get; } = ScaleDegrees.Create(new ScaleDegree[] { 1, 2, 3, 4, 5, 7});
    public static ScaleDegrees Thirteenth { get; } = ScaleDegrees.Create(new ScaleDegree[] { 1, 2, 3, 4, 5, 6, 7});
}