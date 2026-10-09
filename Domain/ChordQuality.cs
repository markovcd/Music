using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Domain;

/// <summary>
/// One note of a chord quality: how many semitones it is above the root and how many letter names
/// above the root's letter it is spelled (a third is 2 letters up, a fifth 4, a seventh 6, and so on).
/// </summary>
public readonly record struct ChordTone(int Semitones, int LetterSteps);

/// <summary>
/// A kind of chord, such as minor seventh, defined by a symbol ("m7") and a formula ("1 b3 5 b7").
/// The formula lists the chord tones as scale degrees counted from the root, each optionally
/// lowered ("b") or raised ("#") by a semitone. Degrees can be 1-7, 9, 11 or 13.
/// </summary>
public sealed class ChordQuality
{
    private static readonly Regex TonePattern = new(@"^(?<accidentals>[#b]*)(?<degree>\d+)$", RegexOptions.Compiled);

    // Semitones above the root of each degree in the major scale (extensions wrap into one octave).
    private static readonly IReadOnlyDictionary<int, int> MajorScaleSemitones = new Dictionary<int, int>
    {
        [1] = 0, [2] = 2, [3] = 4, [4] = 5, [5] = 7, [6] = 9, [7] = 11, [9] = 14, [11] = 17, [13] = 21,
    };

    public ChordQuality(string symbol, string formula)
    {
        Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        Formula = formula ?? throw new ArgumentNullException(nameof(formula));
        Tones = Parse(formula);
        Intervals = Intervals.Create(Tones.Select(t => new Interval(t.Semitones)));
    }

    /// <summary>The text that follows the root in a chord name, for example "m7" in "Am7". Empty for a major triad.</summary>
    public string Symbol { get; }

    public string Formula { get; }

    /// <summary>The chord tones in formula order. The first one is always the root.</summary>
    public IReadOnlyList<ChordTone> Tones { get; }

    /// <summary>The set of semitones above the root that the chord is made of.</summary>
    public Intervals Intervals { get; }

    /// <summary>True if the chord has a minor third, which decides how roots like C#/Db are spelled.</summary>
    internal bool HasMinorThird => Tones.Any(t => t is { LetterSteps: 2, Semitones: 3 });

    public override string ToString()
    {
        return $"{Symbol} ({Formula})";
    }

    private static ImmutableArray<ChordTone> Parse(string formula)
    {
        var tokens = formula.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
            throw new FormatException("A chord formula needs at least the root, \"1\".");

        var tones = tokens.Select(ParseTone).ToImmutableArray();

        if (tokens[0] != "1")
            throw new FormatException($"A chord formula must start with the root \"1\", not \"{tokens[0]}\".");

        if (tones.Select(t => t.Semitones).Distinct().Count() != tones.Length)
            throw new FormatException($"The chord formula \"{formula}\" has two tones on the same note.");

        return tones;
    }

    private static ChordTone ParseTone(string token)
    {
        var match = TonePattern.Match(token);

        if (!match.Success
            || !int.TryParse(match.Groups["degree"].Value, out var degree)
            || !MajorScaleSemitones.TryGetValue(degree, out var semitones))
            throw new FormatException($"\"{token}\" is not a chord tone. Use a degree (1-7, 9, 11 or 13) with optional # or b before it.");

        var accidentals = match.Groups["accidentals"].Value;
        var alteration = accidentals.Count(c => c == '#') - accidentals.Count(c => c == 'b');

        return new ChordTone(Math.Modulo(semitones + alteration, Note.TotalNotes), (degree - 1) % 7);
    }
}
