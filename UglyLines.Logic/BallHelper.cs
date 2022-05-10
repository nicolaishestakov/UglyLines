namespace UglyLines.Logic;

public static class BallHelper
{
    public static Random Random { get; set; } = new Random();
    public static BallColor GetRandomBallColor()
    {
        var colorIndex = Random.Next(BallColors.GetLowerBound(0), BallColors.GetUpperBound(0) +1);
        return BallColors[colorIndex];
    }
    
    private static readonly BallColor[] BallColors = Enum.GetValues<BallColor>();
}