namespace UglyLines.Logic;

public interface INextMoveBalls
{
    IEnumerable<IBall> GetBallsForNextMove();
    IEnumerable<BallXY> ThrowBalls(IEnumerable<IBall> balls, IEnumerable<Location> availableLocations);
}