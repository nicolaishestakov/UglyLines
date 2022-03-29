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
}