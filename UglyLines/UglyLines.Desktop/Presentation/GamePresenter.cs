using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using UglyLines.Desktop.Views;
using UglyLines.Logic;
using Location = UglyLines.Logic.Location;

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

        Game = new Game(fieldWidth, fieldHeight, new NextMoveRandomBalls(new ShapeBallFactory(FieldSettings.CellSize)));
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
                //Ball move will be handled in Field event handler
                //todo here could be moving animation
                //when it finishes: 
                EndMakingMove();
                break;
            case GameState.ClearingLines:
                ProceedToClearingLines();
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
        List<Shape> ballsToAppear = new List<Shape>();

        foreach (var ballXy in Game.BallsToShoot)
        {
            if (ballXy.Ball is ShapeBall shapeBall)
            {
                shapeBall.PutToCanvas(_canvas, 
                    FieldSettings.FieldToCanvasX(ballXy.Location.X),  //todo similar code is in FieldOnBallAdded/Moved
                    FieldSettings.FieldToCanvasY(ballXy.Location.Y)); //when abstracting _canvas, encapsulate this logic as well
                
                ballsToAppear.Add(shapeBall.Shape);
            }
        }

        void OnAppearBallsAnimationsFinished(object? sender, EventArgs args)
        {
            if (sender is AnimationController ac)
            {
                ac.AllAnimationsFinished -= OnAppearBallsAnimationsFinished;
            }

            AfterAnimationContinuation();
        }
        
        if (ballsToAppear.Any())
        {
            StartBallAppearAnimation(ballsToAppear, OnAppearBallsAnimationsFinished);
        }
        else
        {
            AfterAnimationContinuation();
        }

        // When the animation finishes:
        void AfterAnimationContinuation()
        {
            Game.FinishShootingBalls();            
        }
        
    }

    private void ProceedToClearingLines()
    {
        var ballsToClear = Game.BallsToClear.Select(b => b.Ball).OfType<ShapeBall>().ToList();

        void OnClearLinesAnimationsFinished(object? sender, EventArgs args)
        {
            if (sender is AnimationController ac)
            {
                ac.AllAnimationsFinished -= OnClearLinesAnimationsFinished;
            }

            AfterAnimationContinuation();
        }

        if (ballsToClear.Any())
        {
            StartBallDisappearAnimation(ballsToClear, OnClearLinesAnimationsFinished);
        }
        else
        {
            AfterAnimationContinuation();
        }

        void AfterAnimationContinuation()
        {
            Game.NextMoveOrEndGame();
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

   
    // Animations

    private AnimationController _animationController;
    
    public void StartBallAppearAnimation(IEnumerable<Shape> balls, EventHandler<EventArgs>? onAllAnimationsFinished)
    {
        bool animationsStarted = false;
        
        foreach (var ball in balls)
        {
            if (ball is not Ellipse ellipse) continue;
            
            var ballAnimation = new BallAppearAnimation(ellipse, FieldSettings.CellSize* 0.8); 
            //todo DRY, the CellSize*0.8 was copied from DrawBall method 
            _animationController.AddAnimation(ballAnimation);
            animationsStarted = true;
        }

        if (onAllAnimationsFinished != null)
        {
            if (animationsStarted)
            {
                _animationController.AllAnimationsFinished += onAllAnimationsFinished;
            }
            else
            {
                onAllAnimationsFinished.Invoke(null, EventArgs.Empty);
            }
        }

        _animationController.Tick(); //todo smelly
    }
        
        
    public void StartBallDisappearAnimation(
        IEnumerable<ShapeBall> balls, 
        EventHandler<EventArgs>? onAllAnimationsFinished)
    {
        bool animationsStarted = false;
        
        foreach (var ball in balls)
        {
            if (ball.Shape is not Ellipse ellipse) continue;
            
            var ballAnimation = new BallDisappearAnimation(ellipse, FieldSettings.CellSize* 0.8); 
            //todo DRY, the CellSize*0.8 was copied from DrawBall method 

            _animationController.AddAnimation(ballAnimation);
            animationsStarted = true;
        }

        if (onAllAnimationsFinished != null)
        {
            if (animationsStarted)
            {
                _animationController.AllAnimationsFinished += onAllAnimationsFinished;
            }
            else
            {
                onAllAnimationsFinished.Invoke(null, EventArgs.Empty);
            }
        }
    }

    public void Restart()
    {
        Game.Restart();
    }
}