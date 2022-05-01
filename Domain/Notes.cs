using System.Collections;
using System.Collections.Immutable;

namespace Domain;

public record Notes : IReadOnlyCollection<Note>
{
    private readonly ImmutableHashSet<Note> notes;
    
    public Notes(IEnumerable<Note> notes)
    {
        this.notes = notes.ToImmutableHashSet();
    }
    
    public static Notes FromIntervals(Note root, IEnumerable<Interval> intervals)
    {
        return new Notes(intervals.Select(i => root + i).Append(root));
    }

    public Notes Transpose(Interval interval)
    {
        return new Notes(notes.Select(n => n.Transpose(interval)));
    }
    
    public IEnumerator<Note> GetEnumerator()
    {
        return notes.OrderBy(n => n).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => notes.Count;
}