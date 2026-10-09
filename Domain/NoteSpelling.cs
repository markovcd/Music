namespace Domain;

/// <summary>
/// Chooses letter names for notes. <see cref="Note"/> only knows pitch classes, so the same
/// key can be written C# or Db; this is where that choice is made for chord names.
/// </summary>
internal static class NoteSpelling
{
    private const string Letters = "CDEFGAB";
    private static readonly int[] NaturalNotes = { 0, 2, 4, 5, 7, 9, 11 };

    // The usual names for chord roots: flats for Eb and Bb, F# rather than Gb.
    // C#/Db and G#/Ab depend on the chord: C#m and G#m, but Db and Ab.
    private static readonly string[] MajorRoots = { "C", "Db", "D", "Eb", "E", "F", "F#", "G", "Ab", "A", "Bb", "B" };
    private static readonly string[] MinorRoots = { "C", "C#", "D", "Eb", "E", "F", "F#", "G", "G#", "A", "Bb", "B" };

    /// <summary>The conventional name of a chord's root note. <paramref name="minorThird"/> is true for chords like Cm, Cdim, Cm7b5.</summary>
    internal static string SpellRoot(Note root, bool minorThird)
    {
        return (minorThird ? MinorRoots : MajorRoots)[root];
    }

    /// <summary>
    /// Spells a note that is a chord tone <paramref name="letterSteps"/> letters above the root,
    /// so that, for example, the minor third of C is Eb and the diminished fifth of Eb is Gb.
    /// Double sharps and flats are avoided: where the strict spelling needs one (the diminished seventh
    /// of Eb is Dbb, the augmented fifth of F# is C##) the conventional name is used instead (C and D).
    /// </summary>
    internal static string SpellTone(string rootName, int letterSteps, Note note, bool minorThird)
    {
        var rootLetter = Letters.IndexOf(rootName[0]);
        var letterIndex = (rootLetter + letterSteps) % Letters.Length;

        var offset = Math.Modulo((int)note - NaturalNotes[letterIndex] + 6, Note.TotalNotes) - 6;

        if (offset is < -1 or > 1)
            return SpellRoot(note, minorThird);

        var accidental = offset switch { 1 => "#", -1 => "b", _ => "" };

        return $"{Letters[letterIndex]}{accidental}";
    }
}
