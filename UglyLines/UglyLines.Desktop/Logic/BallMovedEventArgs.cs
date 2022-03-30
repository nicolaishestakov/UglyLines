using System;

namespace UglyLines.Desktop.Logic;

public class BallMovedEventArgs: EventArgs
{
    public BallMovedEventArgs(IBall ball, Location from, Location to)
    {
        Ball = ball;
        From = from;
        To = to;
    }

    public IBall Ball { get; }
    public Location From { get;  }
    public Location To { get;  }
}