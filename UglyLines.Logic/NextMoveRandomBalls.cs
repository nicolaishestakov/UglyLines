namespace UglyLines.Logic;

public class NextMoveRandomBalls : INextMoveBalls
{
    public NextMoveRandomBalls(IBallFactory ballFactory)
    {
        _ballFactory = ballFactory;
    }

    public virtual IEnumerable<IBall> GetBallsForNextMove()
    {
        for (var i = 0; i < 3; i++)
        {
            var color = BallHelper.GetRandomBallColor();
            yield return _ballFactory.CreateBall(color);
        }
    }

    public virtual IEnumerable<BallXY> ThrowBalls(IEnumerable<IBall> balls, IEnumerable<Location> availableLocations)
    {
        var emptyCells = availableLocations.ToList();

        foreach (var ball in balls)
        {
            if (!emptyCells.Any())
            {
                yield break;
            }

            var randomCellIndex = _random.Next(0, emptyCells.Count - 1);

            yield return new BallXY(ball, emptyCells[randomCellIndex]);
            
            emptyCells.RemoveAt(randomCellIndex);
        }
    }

    private readonly Random _random = new Random();

    protected readonly IBallFactory _ballFactory;
}