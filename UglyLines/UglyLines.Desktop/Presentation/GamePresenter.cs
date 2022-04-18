using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using UglyLines.Desktop.Views;
using UglyLines.Desktop.Logic;
using Location = UglyLines.Desktop.Logic.Location;

namespace UglyLines.Desktop.ViewModels;

/// <summary>
/// In this state the presentation logic is shared between ViewModel in this class
/// The question is: why have different classes, maybe put everything into one vew model?
/// We can make GamePresenter UI-framework agnostic and use in both Avalonia and WPF implementations
/// It will require further refactoring though
/// </summary>
public class GamePresenter
{
    public GamePresenter(FieldSettings fieldSettings, int fieldWidth, int fieldHeight, AnimationController animationController, Canvas canvas)
    {
        FieldSettings = fieldSettings;
        FieldWidth = fieldWidth; //todo redundancy with FieldSettings
        FieldHeight = fieldHeight;
        _animationController = animationController;
        _canvas = canvas;

        Game = new Game(fieldWidth, fieldHeight);
        Game.GameStateChanged += OnGameStateChanged;
        Game.Field.BallAdded += FieldOnBallAdded;
        Game.Field.BallMoved += FieldOnBallMoved;
        Game.Field.BallsRemoved += FieldOnBallsRemoved;
        
        Game.SelectedBallChanged += GameOnSelectedBallChanged;
    }

    private void GameOnSelectedBallChanged(object? sender, Game.SelectedBallChangedEventArgs e)
    {
        if (e.OldSelectedBall?.Ball is ShapeBall unselectedBall)
        {
            unselectedBall.DrawAsUnselected();
        }
        if (e.NewSelectedBall?.Ball is ShapeBall selectedBall)
        {
            selectedBall.DrawAsSelected();
        }
    }

    private readonly Canvas _canvas; //todo this is not good to have canvas here, because it depends on UI framework.
    private void FieldOnBallsRemoved(object? sender, BallsRemovedEventArgs e)
    {
        foreach (var ball in e.RemovedBalls.Select(bxy => bxy.Ball).OfType<ShapeBall>())
        {
            ball.RemoveFromCanvas();
        }
    }

    private void FieldOnBallMoved(object? sender, BallMovedEventArgs e)
    {
        if (e.Ball is ShapeBall ball)
        {
            ball.SetPositionOnCanvas(
                FieldSettings.FieldToCanvasX(e.To.X),
                FieldSettings.FieldToCanvasY(e.To.Y));
        }
        
    }

    private void FieldOnBallAdded(object? sender, BallAddedEventArgs e)
    {
        if (e.BallXy.Ball is ShapeBall ball)
        {
            ball.PutToCanvas(
                _canvas, 
                FieldSettings.FieldToCanvasX(e.BallXy.Location.X),
                FieldSettings.FieldToCanvasY(e.BallXy.Location.Y));
        }
    }

    private void OnGameStateChanged(object? sender, Game.GameStateChangedEventArgs e)
    {
        var newState = e.NewState;

        switch (newState)
        {
            case GameState.WaitingForSelection:
                break;
            case GameState.BallSelected:
                //Ball selection will be handled in event handler
                break;
            case GameState.BallMoving:
                //Ball move will be handled in Field event hadler
                //todo here could be moving animation
                //when it finishes: 
                EndMakingMove();
                break;
            case GameState.ClearingLines:
                ProceedToClearingLines(e.OldState);
                break;
            case GameState.ShootingNewBalls:
                ProceedToShootingNewBalls();
                break;
            case GameState.GameOver:
                ProceedToGameOver();
                break;
        }
    }

    private void ProceedToShootingNewBalls()
    {
        //todo
        // Game.BallsToShoot animation should be started here 
        // Here will be a problem: Field will add the balls only at the next game state 
        // But the animation will have to draw them on canvas
        // When the animation finishes:
        Game.FinishShootingBalls();
    }
    
    private void ProceedToClearingLines(GameState previousState)
    {
        // todo
        // to make animation work, the next game state must start after the animation is finished  
        // var ballsToClear = Game.BallsToClear.Select(b => b.Ball).OfType<ShapeBall>().ToList();
        //
        // if (ballsToClear.Any())
        // {
        //     StartBallDisappearAnimation(ballsToClear, null);                
        // }

        if (previousState == GameState.BallMoving)
        {
            Game.ShootNewBalls();
        }
        else if (previousState == GameState.ShootingNewBalls)
        {
            Game.NextMoveOrEndGame(GetNextBalls());
        }
    }

    void ProceedToGameOver()
    {
        //todo implement Game Over message
    }

    public int FieldWidth { get; }
    public int FieldHeight { get; }

    public GameState State => Game.GameState;
    
    public Game Game { get; private set; }
    
    public FieldSettings FieldSettings { get; }

    public bool SelectBall(int x, int y)
    {
        return Game.SelectBall(new Location(x, y));
    }

    public bool StartMakingMove(int x, int y)
    {
        return Game.StartMove(new Location(x, y));
    }

    public void EndMakingMove()
    {
        Game.FinishMove();
    }

    private static readonly BallColor[] _ballColors = Enum.GetValues<BallColor>();
    
    public BallColor GetNextBallColor()
    {
        var random = new Random();
        var colorIndex = random.Next(_ballColors.GetLowerBound(0), _ballColors.GetUpperBound(0) + 1);

        return _ballColors[colorIndex];
    }

    private IEnumerable<IBall> GetNextBalls()
    {
        //todo ball generation can be delegated into logic layer
        var b1 = new ShapeBall(GetNextBallColor(), FieldSettings.CellSize);
        var b2 = new ShapeBall(GetNextBallColor(), FieldSettings.CellSize);
        var b3 = new ShapeBall(GetNextBallColor(), FieldSettings.CellSize);

        return new[] { b1, b2, b3 };
    }
    
    // Animations

    private AnimationController _animationController;
    
    public void StartBallAppearAnimation(IEnumerable<Shape> balls)
    {
        foreach (var ball in balls)
        {
            if (ball is Ellipse ellipse)
            {
                var ballAnimation = new BallAppearAnimation(ellipse, FieldSettings.CellSize* 0.8); 
                //todo DRY, the CellSize*0.8 was copied from DrawBall method 
                _animationController.AddAnimation(ballAnimation);
            }
        }
            
        _animationController.Tick();
    }
        
        
    public void StartBallDisappearAnimation(IEnumerable<ShapeBall> balls, Action<Animation>? onAnimationFinished)
    {
        foreach (var ball in balls)
        {
            if (ball.Shape is Ellipse ellipse)
            {
                var ballAnimation = new BallDisappearAnimation(ellipse, FieldSettings.CellSize* 0.8); 
                //todo DRY, the CellSize*0.8 was copied from DrawBall method 

                if (onAnimationFinished != null)
                {
                    EventHandler<AnimationFinishedEventArgs> animationFinishedHandler = (object? sender, AnimationFinishedEventArgs args) =>
                    {
                        onAnimationFinished?.Invoke(ballAnimation);
                    };
                    ballAnimation.AnimationFinished += animationFinishedHandler;
                }
                _animationController.AddAnimation(ballAnimation);
            }
        }
    }

    public void Restart()
    {
        Game.Restart(GetNextBalls(), GetNextBalls());
    }
}