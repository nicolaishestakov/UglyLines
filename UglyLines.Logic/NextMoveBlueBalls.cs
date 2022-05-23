namespace UglyLines.Logic;

public class NextMoveBlueBalls : NextMoveRandomBalls
{
    public NextMoveBlueBalls(IBallFactory ballFactory) : base(ballFactory)
    {
    }

    public override IEnumerable<IBall> GetBallsForNextMove()
    {
        for (var i = 0; i < 3; i++)
        {
            yield return _ballFactory.CreateBall(BallColor.Blue);
        }
    }
    
}