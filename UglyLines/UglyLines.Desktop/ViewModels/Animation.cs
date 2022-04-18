using System;

namespace UglyLines.Desktop.ViewModels; //todo it is not quite a view model, later this will be moved to a diff namespace

public abstract class Animation
{
    protected Animation(int endStep)
    {
        Step = 0;
        EndStep = endStep;
    }
    public int Step { get; private set; }
    public int EndStep { get; }

    public bool IsOver()
    {
        return Step >= EndStep;
    }

    public abstract void ProceedToNextStep();

    protected void IncrementStep()
    {
        if (Step < EndStep)
        {
            Step++;

            if (IsOver())
            {
                AnimationFinished?.Invoke(this, new AnimationFinishedEventArgs(this));
            }
        }
    }
    
    public event EventHandler<AnimationFinishedEventArgs>? AnimationFinished;
}