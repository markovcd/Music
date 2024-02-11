using System.Collections;
using System.Collections.Immutable;

namespace Domain;

public readonly record struct Intervals : IEnumerable<Interval>, IComparable<Intervals>
{
  public Intervals(int value)
  {
    if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));

    Value = value;
  }

  public int Value { get; }
  
  public IEnumerable<Pitch> GetPitches(Pitch root)
  {
    return GetIntervals().Select(i => root + i);
  }
  
  public IEnumerable<Note> GetNotes(Note root)
  {
    return GetIntervals().Select(i => root + i);
  }

  public static Intervals Create(IEnumerable<Interval> intervals)
  {
    intervals = intervals.Distinct().ToImmutableList();

    if (intervals.Any(i => (int)i is > 31 or < 0)) 
      throw new ArgumentOutOfRangeException(nameof(intervals));
    
    return new Intervals(intervals.Distinct()
      .Select(i => (int)System.Math.Pow(2, i))
      .Sum());
  }

  public static Intervals Create(Pitch root, IEnumerable<Pitch> pitches)
  {
    return Create(pitches.Select(p => p - root));
  }
  
  public bool HasInterval(Interval interval)
  {
    return GetIntervals().Contains(interval);
  }
  
  public bool HasDegree(Degree degree)
  {
    return degree <= GetIntervals().Count;
  }

  public Interval GetInterval(Degree degree)
  {
    var intervals = GetIntervals();
    
    if (degree > intervals.Count)
      throw new ArgumentOutOfRangeException(nameof(degree), degree, null);
    
    return intervals[degree - 1];
  }

  public Intervals GetIntervals(Degrees degrees)
  {
    var intervals = GetIntervals();

    if (degrees.Max() > intervals.Count)
      throw new ArgumentOutOfRangeException(nameof(degrees), degrees, null);

    return Create(degrees.Select(d => intervals[d - 1]));
  }
  
  public Intervals Normalize()
  {
    return Create(GetIntervals().Select(i => i.Normalize()));
  }

  public Intervals AddIntervals(Intervals intervals)
  {
    return new Intervals(this | intervals);
  }
  
  public Intervals RemoveIntervals(Intervals intervals)
  {
    return new Intervals(this ^ (this & intervals));
  }

  public int CompareTo(Intervals other)
  {
    return Value.CompareTo(other.Value);
  }

  public IEnumerator<Interval> GetEnumerator()
  {
    return GetIntervals().GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  private IReadOnlyList<Interval> GetIntervals()
  {
    return new BitArray(new[] { Value })
      .Cast<bool>()
      .Select((b, i) => (b, i))
      .Where(t => t.b)
      .Select(t => new Interval(t.i))
      .OrderBy(i => i)
      .ToImmutableList();
  }
  
  public Intervals Transform(Degree degree)
  {
    AssertDegree(degree);
    
    var intervals = this.ToList();
    var newRoot = intervals[degree - 1];
        
    return Create(
      intervals.Select(i => new Interval(
        Math.Modulo(i - newRoot, Note.TotalNotes))));
  }
  
  public Chord GetChord(Degree root, Degrees template)
  {
    if (!template.All(HasDegree)) 
      throw new ArgumentOutOfRangeException(nameof(template), template, null);

    var transformed = Transform(root);
    var chordSteps = template.Select(d => new ChordStep(d, transformed.GetInterval(d)));

    return new Chord(chordSteps);
  }
  
  private void AssertDegree(Degree degree)
  {
    if (degree > this.Count())
      throw new ArgumentOutOfRangeException(nameof(degree), degree, null);
  }
  
  public override string ToString()
  {
    return Value.ToString();
  }

  public static implicit operator int(Intervals intervals)
  {
    return intervals.Value;
  }

  public static explicit operator Intervals(int i)
  {
    return new Intervals(i);
  }
  
  public static bool operator >(Intervals first, Intervals second)
  {
    return first.CompareTo(second) > 0;
  }
    
  public static bool operator >=(Intervals first, Intervals second)
  {
    return first.CompareTo(second) >= 0;
  }
    
  public static bool operator <(Intervals first, Intervals second)
  {
    return second > first;
  }
    
  public static bool operator <=(Intervals first, Intervals second)
  {
    return second >= first;
  }
  
  public static Intervals operator +(Intervals first, Intervals second)
  {
    return first.AddIntervals(second);
  }
  
  public static Intervals operator +(Intervals first, Interval second)
  {
    return first.AddIntervals(Create(new[] { second }));
  }
    
  public static Intervals operator -(Intervals first, Intervals second)
  {
    return first.RemoveIntervals(second);
  }
  
  public static Intervals operator -(Intervals first, Interval second)
  {
    return first.RemoveIntervals(Create(new[] { second }));
  }
}
