using UglyLines.Logic;

internal class BallFactory : IBallFactory
{
    public IBall CreateBall(BallColor color)
    {
        return new Ball { Color = color };
    }
}