using System.Collections;

namespace Domain;

public class KnownIntervals : IReadOnlyDictionary<string, Scale>
{
    private static IReadOnlyDictionary<string, Scale> d = new Dictionary<string, Scale>
    {
        { "Diminished Triad", new Scale(73) },
        { "Diminished Triad", new Scale(73) },
        { "Minor Triad", new Scale(137) },
        { "Major triad", new Scale(145) },
        { "Augmented Triad", new Scale(273) },
    };

    public IEnumerator<KeyValuePair<string, Scale>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count { get; }
    public bool ContainsKey(string key)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(string key, out Scale value)
    {
        throw new NotImplementedException();
    }

    public Scale this[string key] => throw new NotImplementedException();

    public IEnumerable<string> Keys { get; }
    public IEnumerable<Scale> Values { get; }
}