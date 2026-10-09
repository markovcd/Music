namespace Domain;

/// <summary>
/// How hard a shape is to play: the number of fingers it needs, whether one of them barres,
/// how far the hand stretches and how far up the neck it is.
/// </summary>
/// <param name="Fingers">The fewest fingers needed. A finger can lie flat across strings next to each other at the same fret,
/// and a barre covers the lowest fret across the strings, as long as none of the strings in between is open or muted.</param>
/// <param name="PressedNotes">The fretted notes that are not covered by a barre. Each one has to be pressed down on its own.</param>
/// <param name="BarreFret">The fret of the barre, or null if no finger barres.</param>
/// <param name="Span">The number of frets from the lowest to the highest fretted note, counting both. 0 if nothing is fretted.</param>
/// <param name="HighestFret">The highest fretted note. 0 if nothing is fretted.</param>
/// <param name="MutedStrings">The number of strings that do not sound.</param>
public sealed record Fingering(int Fingers, int PressedNotes, int? BarreFret, int Span, int HighestFret, int MutedStrings)
{
    /// <summary>
    /// A score to compare shapes with, lower is easier: 3 for each note that is pressed down, 5 for a barre,
    /// 2 for each fret of stretch, 1 for each fret up the neck and 4 for each muted string,
    /// so a full open chord comes before a few notes of it.
    /// </summary>
    public int Difficulty => 3 * PressedNotes + (BarreFret.HasValue ? 5 : 0) + 2 * Span + HighestFret + 4 * MutedStrings;

    /// <summary>Analyses the frets that sound on the board (the highest pressed fret of each string).</summary>
    public static Fingering Analyze(Fretboard board)
    {
        return Analyze(Enumerable.Range(0, board.StringCount)
            .Select(i => board.GetFret(i) is { } fret ? (int?)fret : null)
            .ToList());
    }

    /// <summary>Analyses a shape given as the fret of each string in string order, null for a muted string and 0 for an open one.</summary>
    public static Fingering Analyze(IReadOnlyList<int?> frets)
    {
        var muted = frets.Count(f => f is null);
        var fretted = Enumerable.Range(0, frets.Count).Where(i => frets[i] > 0).ToList();

        if (fretted.Count == 0) return new Fingering(0, 0, null, 0, 0, muted);

        var lowest = fretted.Min(i => frets[i]!.Value);
        var highest = fretted.Max(i => frets[i]!.Value);

        var fingers = 0;
        int? barre = null;
        var covered = new HashSet<int>();

        var onLowest = fretted.Where(i => frets[i] == lowest).ToList();

        if (onLowest.Count >= 2)
        {
            var first = onLowest.Min();
            var last = onLowest.Max();
            var clear = Enumerable.Range(first, last - first + 1).All(i => frets[i] >= lowest);

            if (clear)
            {
                barre = lowest;
                fingers++;
                covered.UnionWith(onLowest);
            }
        }

        var previousIndex = -2;
        int? previousFret = null;

        foreach (var index in fretted.Where(i => !covered.Contains(i)))
        {
            var fret = frets[index]!.Value;

            if (index != previousIndex + 1 || fret != previousFret)
                fingers++;

            previousIndex = index;
            previousFret = fret;
        }

        return new Fingering(fingers, fretted.Count - covered.Count, barre, highest - lowest + 1, highest, muted);
    }
}
