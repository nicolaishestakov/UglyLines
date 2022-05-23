namespace UglyLines.Logic;

public interface IColorField
{
    BallColor? GetBallColor(Location xy);
    public int Width { get; }
    public int Height { get; }
}