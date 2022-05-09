using System.Collections;
using System.Collections.Immutable;

namespace Domain;

public readonly record struct Pitches(Pitch Root, Intervals Intervals) : IEnumerable<Pitch>
{
  public static Pitches Create(IEnumerable<Pitch> pitches)
  {
    pitches = pitches.Distinct().OrderBy(p => p).ToImmutableList();
    
    var root = pitches.First();
    
    return new Pitches(root, Intervals.Create(root, pitches));
  }
  
  public Pitches Transpose(Interval interval)
  {
    return this with { Root = Root + interval };
  }

  public bool HasPitch(Pitch pitch)
  {
    return GetPitches().Contains(pitch);
  }
  
  public bool HasNote(Note note)
  {
    return GetPitches().Any(i => i.Note == note);
  }
  
  public IEnumerator<Pitch> GetEnumerator()
  {
    return GetPitches().GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
  
  private IEnumerable<Pitch> GetPitches()
  {
    return Intervals.GetPitches(Root);
  }
}
