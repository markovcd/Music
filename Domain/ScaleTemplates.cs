namespace Domain;

public static class ScaleTemplates
{
    public static Scale Minor { get; } = Scale.Create(new Interval[] { 0, 2, 3, 5, 7, 8, 10 });
    public static Scale Major { get; } = Minor.Transform(3);
}