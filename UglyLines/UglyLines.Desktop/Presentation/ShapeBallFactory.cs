using UglyLines.Logic;

namespace UglyLines.Desktop.ViewModels;

public class ShapeBallFactory: IBallFactory
{
    public ShapeBallFactory(double cellSize)
    {
        _cellSize = cellSize;
    }

    public IBall CreateBall(BallColor color)
    {
        return new ShapeBall(color, _cellSize);
    }

    private readonly double _cellSize;
}