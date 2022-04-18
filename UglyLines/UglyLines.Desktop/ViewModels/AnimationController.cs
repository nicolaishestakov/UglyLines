using System;
using System.Collections.Generic;
using System.Linq;

namespace UglyLines.Desktop.ViewModels;

public class AnimationController
{
    public IEnumerable<Animation> ActiveAnimations => ActiveAnimationList;
    private List<Animation> ActiveAnimationList { get; } = new();

    public void Tick()
    {
        if (!ActiveAnimationList.Any())
        {
            return;
        }
        
        var finishedAnimations = new List<Animation>();
        foreach (var animation in ActiveAnimationList)
        {
            animation.ProceedToNextStep(); //todo one tick is one step, but we may want different step tick counts later

            if (animation.IsOver())
            {
                finishedAnimations.Add(animation);
            }
        }
        
        //remove finished animations
        ActiveAnimationList.RemoveAll(a => finishedAnimations.Contains(a));

        foreach (var animation in finishedAnimations)
        {
            AnimationFinished?.Invoke(this, new AnimationFinishedEventArgs(animation));
        }

        if (!ActiveAnimationList.Any())
        {
            AllAnimationsFinished?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddAnimation(Animation animation)
    {
        ActiveAnimationList.Add(animation);

        if (ActiveAnimationList.Count == 1)
        {
            FirstAnimationStarted?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public event EventHandler<AnimationFinishedEventArgs>? AnimationFinished;

    public event EventHandler<EventArgs>? AllAnimationsFinished;

    public event EventHandler<EventArgs>? FirstAnimationStarted;
}

public class AnimationFinishedEventArgs: EventArgs
{
    public AnimationFinishedEventArgs(Animation animation)
    {
        Animation = animation;
    }
    public Animation Animation { get; }
}