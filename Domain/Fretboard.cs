using System.Collections.Immutable;

namespace Domain;

public readonly record struct Fretboard
{

    

    public bool IsPressed(int stringIndex, Interval interval)
    {
        return false;
    }

    public Fretboard Transpose(Interval interval)
    {
        throw new NotImplementedException();
    }
    
    public Fretboard PressFret(int stringIndex, Interval interval)
    {
        throw new NotImplementedException();
    }
    
    public Fretboard DepressFret(int stringIndex, Interval interval)
    {
        throw new NotImplementedException();
    }
    
    private Fretboard ModifyString(int stringIndex, Func<String, String> action)
    {
        throw new NotImplementedException();

    }

}
