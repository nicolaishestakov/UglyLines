using System;

namespace UglyLines.Desktop.Logic;

public class BallAddedEventArgs: EventArgs
{
    public BallAddedEventArgs(BallXY ballXy)
    {
        BallXy = ballXy;
    }

    public BallXY BallXy { get; }
}