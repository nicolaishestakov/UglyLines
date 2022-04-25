namespace UglyLines.Logic;

public class BallsRemovedEventArgs
{
    public BallsRemovedEventArgs(List<BallXY> removedBalls)
    {
        _removedBalls = removedBalls;
    }
    
    private readonly List<BallXY> _removedBalls;
    public IEnumerable<BallXY> RemovedBalls => _removedBalls;
}