namespace Domain;

/// <summary>
/// The chord qualities <see cref="ChordNamer"/> knows. The order is the order of preference
/// when several qualities could name the same notes with different roots: common chords come first,
/// chords with an omitted fifth last.
/// </summary>
public static class ChordQualities
{
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
