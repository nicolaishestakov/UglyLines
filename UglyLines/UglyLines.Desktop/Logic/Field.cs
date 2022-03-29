using System;
using System.Collections.Generic;

namespace UglyLines.Desktop.Logic;

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
}