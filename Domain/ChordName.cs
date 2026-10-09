using System.Text.RegularExpressions;

namespace Domain;

/// <summary>
/// A chord symbol such as <c>C</c>, <c>Am7</c>, <c>Ebmaj7</c> or <c>G/B</c>.
/// <see cref="Bass"/> is only set for slash chords (when the lowest note is not the root).
/// <see cref="RootName"/> and <see cref="BassName"/> are the letter names used when the chord is written;
/// they default to the sharp spelling of the notes.
/// </summary>
public readonly record struct ChordName(Note Root, string Quality, Note? Bass = null)
{
    private const string AccidentalSymbols = "#b♯♭";

    private static readonly Regex NamePattern = new($"^[A-G][{AccidentalSymbols}]?$", RegexOptions.Compiled);

    public string RootName { get; init; } = Root.ToString();

    public string? BassName { get; init; } = Bass?.ToString();

    /// <summary>
    /// Reads a chord symbol: a root (A-G, optionally followed by # or b), one of the symbols in
    /// <see cref="ChordQualities"/> ("m7", "maj7", "7b9", ...; nothing for a major chord) and optionally
    /// a slash and a bass note, for example "Bbmaj7", "F#m7b5", "C6/9" or "Am7/G".
    /// The names are kept as typed, so "Db" stays Db.
    /// </summary>
    public static bool TryParse(string? text, out ChordName chord)
    {
        chord = default;

        text = text?.Trim();
        if (string.IsNullOrEmpty(text) || text[0] is < 'A' or > 'G') return false;

        var rootLength = text.Length > 1 && AccidentalSymbols.Contains(text[1]) ? 2 : 1;
        var rootName = text[..rootLength];
        var rest = text[rootLength..];

        foreach (var quality in ChordQualities.All)
        {
            var symbol = quality.Symbol;
            string? bassName = null;

            if (rest != symbol)
            {
                if (!rest.StartsWith(symbol + "/", StringComparison.Ordinal)) continue;

                bassName = rest[(symbol.Length + 1)..];
                if (!NamePattern.IsMatch(bassName)) continue;
            }

            var root = Note.Parse(rootName);
            var bass = bassName is null ? (Note?)null : Note.Parse(bassName);

            chord = new ChordName(root, symbol, bass == root ? null : bass)
            {
                RootName = rootName,
                BassName = bass == root ? null : bassName,
            };

            return true;
        }

        return false;
    }

    /// <summary>Like <see cref="TryParse"/> but throws a <see cref="FormatException"/> for text that is not a chord symbol.</summary>
    public static ChordName Parse(string text)
    {
        return TryParse(text, out var chord)
            ? chord
            : throw new FormatException($"\"{text}\" is not a chord symbol.");
    }

    public override string ToString()
    {
        return Bass is { } bass && bass != Root
            ? $"{RootName}{Quality}/{BassName}"
            : $"{RootName}{Quality}";
    }
}
