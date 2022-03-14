using Avalonia.Controls.Shapes;
using UglyLines.Desktop.ViewModels;

namespace UglyLines.Desktop.Views;

public class BallDisappearAnimation : Animation
{
    private const int PhaseCount = 5; 
    public BallDisappearAnimation(Ellipse ball, double maxBallSize): base(PhaseCount)
    {
        _ball = ball;
        _maxBallSize = maxBallSize;
    }

    public Shape Ball => _ball; 
    
    private readonly Ellipse _ball;
    private readonly double _maxBallSize;

    public override void ProceedToNextStep()
    {
        if (IsOver()) return;
        
        IncrementStep();

        _ball.Height = (PhaseCount - Step) * _maxBallSize / PhaseCount;
        _ball.Width = (PhaseCount - Step) * _maxBallSize / PhaseCount;
    }
}