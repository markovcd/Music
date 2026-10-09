namespace Domain;

/// <summary>
/// The chord qualities <see cref="ChordNamer"/> knows. The order is the order of preference
/// when several qualities could name the same notes with different roots: common chords come first,
/// chords with an omitted fifth last.
/// </summary>
public static class ChordQualities
{
    /// <summary>The quality with the given symbol ("m7", "6/9", ...), or null if there is none. The symbol for a major chord is empty.</summary>
    public static ChordQuality? Find(string symbol)
    {
        return All.FirstOrDefault(q => q.Symbol == symbol);
    }

    /// <summary>
    /// Other ways chord symbols are written, each with the symbol it stands for: "M7" and "Δ" for "maj7",
    /// "min7" and "-7" for "m7", "ø" for "m7b5", "+" for "aug" and so on.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Aliases { get; } = new (string Alias, string Symbol)[]
    {
        ("maj", ""), ("major", ""), ("M", ""),
        ("min", "m"), ("minor", "m"), ("mi", "m"), ("-", "m"),
        ("o", "dim"), ("°", "dim"), ("mb5", "dim"),
        ("+", "aug"), ("+5", "aug"),
        ("sus", "sus4"),
        ("min6", "m6"), ("mi6", "m6"), ("-6", "m6"),
        ("69", "6/9"),
        ("m69", "m6/9"), ("min69", "m6/9"),
        ("M7", "maj7"), ("Δ", "maj7"), ("Δ7", "maj7"), ("ma7", "maj7"), ("major7", "maj7"),
        ("min7", "m7"), ("mi7", "m7"), ("-7", "m7"),
        ("mM7", "mMaj7"), ("mmaj7", "mMaj7"), ("minMaj7", "mMaj7"), ("m(maj7)", "mMaj7"), ("m(M7)", "mMaj7"),
        ("-M7", "mMaj7"), ("-maj7", "mMaj7"),
        ("ø", "m7b5"), ("ø7", "m7b5"), ("m7-5", "m7b5"), ("m7(b5)", "m7b5"), ("min7b5", "m7b5"), ("-7b5", "m7b5"),
        ("o7", "dim7"), ("°7", "dim7"),
        ("7(b5)", "7b5"), ("7-5", "7b5"),
        ("+7", "aug7"), ("7+", "aug7"), ("7#5", "aug7"), ("7+5", "aug7"), ("7(#5)", "aug7"),
        ("M7#5", "maj7#5"), ("maj7+", "maj7#5"), ("M7+", "maj7#5"),
        ("7sus", "7sus4"),
        ("9sus", "9sus4"),
        ("(add9)", "add9"), ("add2", "add9"),
        ("m(add9)", "madd9"), ("minadd9", "madd9"), ("-add9", "madd9"), ("madd2", "madd9"),
        ("M9", "maj9"), ("Δ9", "maj9"), ("major9", "maj9"),
        ("min9", "m9"), ("mi9", "m9"), ("-9", "m9"),
        ("min11", "m11"), ("-11", "m11"),
        ("M13", "maj13"), ("Δ13", "maj13"),
        ("min13", "m13"), ("-13", "m13"),
        ("7(b9)", "7b9"), ("7-9", "7b9"),
        ("7(#9)", "7#9"), ("7+9", "7#9"),
        ("7(#11)", "7#11"), ("7+11", "7#11"),
        ("7(b13)", "7b13"),
        ("7no5", "7(no5)"),
        ("maj7no5", "maj7(no5)"), ("M7no5", "maj7(no5)"),
        ("m7no5", "m7(no5)"), ("min7no5", "m7(no5)"),
    }.ToDictionary(a => a.Alias, a => a.Symbol); // throws if an alias is listed twice

    /// <summary>
    /// Every way to write a quality, as the text that follows the root and the symbol it stands for:
    /// the symbols themselves, then the aliases.
    /// </summary>
    internal static IEnumerable<KeyValuePair<string, string>> Spellings =>
        All.Select(q => KeyValuePair.Create(q.Symbol, q.Symbol)).Concat(Aliases);

    public static IReadOnlyList<ChordQuality> All { get; } = new[]
    {
        // Triads and power chord
        new ChordQuality("", "1 3 5"),
        new ChordQuality("m", "1 b3 5"),
        new ChordQuality("dim", "1 b3 b5"),
        new ChordQuality("aug", "1 3 #5"),
        new ChordQuality("sus2", "1 2 5"),
        new ChordQuality("sus4", "1 4 5"),
        new ChordQuality("5", "1 5"),

        // Sixths
        new ChordQuality("6", "1 3 5 6"),
        new ChordQuality("m6", "1 b3 5 6"),
        new ChordQuality("6/9", "1 3 5 6 9"),
        new ChordQuality("m6/9", "1 b3 5 6 9"),

        // Sevenths
        new ChordQuality("7", "1 3 5 b7"),
        new ChordQuality("maj7", "1 3 5 7"),
        new ChordQuality("m7", "1 b3 5 b7"),
        new ChordQuality("mMaj7", "1 b3 5 7"),
        new ChordQuality("m7b5", "1 b3 b5 b7"),
        new ChordQuality("dim7", "1 b3 b5 bb7"),
        new ChordQuality("7b5", "1 3 b5 b7"),
        new ChordQuality("aug7", "1 3 #5 b7"),
        new ChordQuality("maj7#5", "1 3 #5 7"),

        // Suspended sevenths and ninths
        new ChordQuality("7sus4", "1 4 5 b7"),
        new ChordQuality("7sus2", "1 2 5 b7"),
        new ChordQuality("9sus4", "1 4 5 b7 9"),

        // Added notes
        new ChordQuality("add9", "1 3 5 9"),
        new ChordQuality("madd9", "1 b3 5 9"),

        // Ninths, elevenths and thirteenths
        new ChordQuality("9", "1 3 5 b7 9"),
        new ChordQuality("maj9", "1 3 5 7 9"),
        new ChordQuality("m9", "1 b3 5 b7 9"),
        new ChordQuality("11", "1 3 5 b7 9 11"),
        new ChordQuality("m11", "1 b3 5 b7 9 11"),
        new ChordQuality("13", "1 3 5 b7 9 13"),
        new ChordQuality("maj13", "1 3 5 7 9 13"),
        new ChordQuality("m13", "1 b3 5 b7 9 13"),

        // Altered dominants
        new ChordQuality("7b9", "1 3 5 b7 b9"),
        new ChordQuality("7#9", "1 3 5 b7 #9"),
        new ChordQuality("7#11", "1 3 5 b7 #11"),
        new ChordQuality("7b13", "1 3 5 b7 b13"),

        // Common guitar voicings that leave the fifth out
        new ChordQuality("7(no5)", "1 3 b7"),
        new ChordQuality("maj7(no5)", "1 3 7"),
        new ChordQuality("m7(no5)", "1 b3 b7"),
    };
}
