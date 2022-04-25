namespace UglyLines.Logic;

public class Field
{
    public Field(int width, int height)
    {
        _cells = new IBall[width, height];
    }

    private readonly IBall?[,] _cells;

    public int Width => _cells.GetLength(0);
    public int Height => _cells.GetLength(1);

    public bool IsWithinBounds(Location xy)
    {
        return xy.X >= 0 && xy.X < Width && xy.Y >= 0 && xy.Y < Height;
    }
    
    public IBall? GetBall(Location xy)
    {
        if (!IsWithinBounds(xy))
        {
            throw new ArgumentOutOfRangeException(nameof(xy));
        }

        return _cells[xy.X, xy.Y];
    }

    public IEnumerable<BallXY> GetBalls()
    {
        var w = Width;
        var h = Height;
        
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
        {
            var location = new Location(x, y);
            var ball = GetBall(location);
            if (ball != null)
            {
                yield return new BallXY(ball, location);
            }
        }
    }

    public IEnumerable<Location> GetEmptyCells()
    {
        var w = Width;
        var h = Height;
        
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
        {
            var location = new Location(x, y);
            var ball = GetBall(location);
            if (ball == null)
            {
                yield return location;
            }
        }
    }

    public event EventHandler<BallAddedEventArgs>? BallAdded;

    public event EventHandler<BallMovedEventArgs>? BallMoved;

    public event EventHandler<BallsRemovedEventArgs>? BallsRemoved;

    public void AddBall(BallXY ballXY)
    {
        if (GetBall(ballXY.Location) != null)
        {
            throw new ArgumentException("Cell is already occupied", nameof(ballXY));
        }

        _cells[ballXY.Location.X, ballXY.Location.Y] = ballXY.Ball;
        
        BallAdded?.Invoke(this, new BallAddedEventArgs(ballXY));
    }

    public void MoveBall(Location from, Location to)
    {
        var ball = GetBall(from);

        if (ball == null)
        {
            throw new ArgumentException($"The cell {from.X}, {from.Y} is empty");
        }
        
        if (GetBall(to) != null)
        {
            throw new ArgumentException("Cell is already occupied", nameof(to)); //todo duplicate message
        }

        _cells[from.X, from.Y] = null;
        _cells[to.X, to.Y] = ball;
        
        BallMoved?.Invoke(this, new BallMovedEventArgs(ball, from, to));
    }

    public void Clear()
    {
        var balls = new List<BallXY>();
        
        var w = Width;
        var h = Height;
        
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
        {
            var location = new Location(x, y);
            var ball = GetBall(location);
            if (ball != null)
            {
                balls.Add(new BallXY(ball, location));
                _cells[x, y] = null;
            }
        }
        
        BallsRemoved?.Invoke(this, new BallsRemovedEventArgs(balls));
    }

    public void RemoveBalls(IEnumerable<Location> locationsToClear)
    {
        var balls = new List<BallXY>();
        
        foreach (var location in locationsToClear)
        {
            var ball = GetBall(location);
            
            if (ball != null)
            {
                balls.Add(new BallXY(ball, location));
                _cells[location.X, location.Y] = null;
            }
        }

        if (balls.Any())
        {
            BallsRemoved?.Invoke(this, new BallsRemovedEventArgs(balls));
        }
    }
}