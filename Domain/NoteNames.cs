using System.Collections;

namespace Domain;

/// <summary>
/// The letter names to use for notes in a key, for example Db Eb F Gb Ab Bb C for Db major.
/// Notes that are not in the key have no name here. Create one with <see cref="Scale.GetNoteNames"/>.
/// </summary>
public sealed class NoteNames : IEnumerable<KeyValuePair<Note, string>>
{
    private readonly IReadOnlyList<KeyValuePair<Note, string>> ordered;
    private readonly IReadOnlyDictionary<Note, string> byNote;

    public NoteNames(IEnumerable<KeyValuePair<Note, string>> names)
    {
        ordered = names.ToList();
        byNote = ordered.ToDictionary(p => p.Key, p => p.Value);
    }

    public int Count => ordered.Count;

    /// <summary>The name of the note in this key, or null if the note is not in the key.</summary>
    public string? Find(Note note)
    {
        return byNote.TryGetValue(note, out var name) ? name : null;
    }

    public IEnumerator<KeyValuePair<Note, string>> GetEnumerator()
    {
        return ordered.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        return string.Join(" ", ordered.Select(p => p.Value));
    }
}
