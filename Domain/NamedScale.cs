namespace Domain;

/// <summary>A scale with a name to show, such as "Dorian".</summary>
public sealed record NamedScale(string Name, Scale Scale)
{
    public override string ToString()
    {
        return Name;
    }
}
