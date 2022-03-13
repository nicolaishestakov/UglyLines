using Avalonia.Controls.Shapes;
using UglyLines.Desktop.ViewModels;

namespace UglyLines.Desktop.Views;

public class BallAppearAnimation: Animation
{
    private const int PhaseCount = 5; 
    public BallAppearAnimation(Ellipse ball, double maxBallSize): base(PhaseCount)
    {
        _ball = ball;
        _maxBallSize = maxBallSize;
    }

    private readonly Ellipse _ball;
    private readonly double _maxBallSize;

    public override void ProceedToNextStep()
    {
        if (IsOver()) return;
        
        IncrementStep();

        _ball.Height = Step * _maxBallSize / PhaseCount; //todo to be done
        _ball.Width = Step * _maxBallSize / PhaseCount;
    }
}