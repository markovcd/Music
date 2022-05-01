using System.Collections;
using System.Collections.Immutable;

namespace Domain;

public sealed record Notes : IReadOnlyCollection<Note>
{
    private readonly ImmutableHashSet<Note> notes;
    
    public Notes(IEnumerable<Note> notes)
    {
        this.notes = notes.ToImmutableHashSet();
    }
    
    public int Count => notes.Count;
    
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
    
    public bool Equals(Notes? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Count == other.Count 
               && this.Zip(other)
                   .All(t => t.First.Equals(t.Second));
    }
    
    public override int GetHashCode()
    {
        const int prime1 = 19;
        const int prime2 = 23;
        
        unchecked 
        {
            var hash = this.Aggregate(
                prime1,
                (current, note) => current * prime2 + note.GetHashCode());

            return hash * prime2 + EqualityContract.GetHashCode();
        }
    }
}