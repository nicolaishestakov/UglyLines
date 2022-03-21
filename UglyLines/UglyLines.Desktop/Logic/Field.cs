namespace UglyLines.Desktop.Logic;

public class Field
{
    
}

public enum BallColor
{
    Red,
    Green,
    Blue,
    Cyan,
    Yellow,
    Brown,
    Magenta
}

public class Ball
{
    public Ball(BallColor color)
    {
        Color = color;
    }
    public BallColor Color { get; }
}

public record BallXY
{
    public Location Location { get; }
    public Ball Ball { get; }
}

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