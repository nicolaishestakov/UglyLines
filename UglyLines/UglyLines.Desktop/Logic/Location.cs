namespace UglyLines.Desktop.Logic;

public struct Location
{
    public Location(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
    public int X { get;  }
    public int Y { get;  }

    public static bool operator ==(Location l1, Location l2)
    {
        return l1.Equals(l2);
    }
    
    public static bool operator !=(Location l1, Location l2)
    {
        return !l1.Equals(l2);
    }
}