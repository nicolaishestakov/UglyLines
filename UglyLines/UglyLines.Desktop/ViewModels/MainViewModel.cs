using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using UglyLines.Desktop.Logic;
using UglyLines.Desktop.Views;

namespace UglyLines.Desktop.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Ugly Lines!";

        public MainViewModel()
        {
            _animationTimer.Tick += (sender, args) =>
            {
                _animationController.Tick();
            };

            _animationController.FirstAnimationStarted += (sender, args) =>
            {
                _animationTimer.Start();
            };

            _animationController.AllAnimationsFinished += (sender, args) =>
            {
                _animationTimer.Stop();
            };
        }
        
        public GamePresenter GamePresenter { get; private set; } = new GamePresenter(new FieldSettings
            {
                LeftMargin = 30,
                TopMargin = 80,
                CellSize = 40,
                Width = 9, //todo DRY
                Height = 9

            },
            9, 9); //todo DRY

        private DispatcherTimer _animationTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(100)};
        //todo the timer is Avalonia dependency, some cross-platform abstraction is needed here
        
        private AnimationController _animationController = new ();
        
        public void StartGame()
        {
            GamePresenter = new GamePresenter(new FieldSettings
                {
                    LeftMargin = 30,
                    TopMargin = 80,
                    CellSize = 40,
                    Width = 9, //todo DRY
                    Height = 9

                },
                9, 9); //todo DRY
        }

        public void StartBallAppearAnimation(IEnumerable<Shape> balls)
        {
            foreach (var ball in balls)
            {
                if (ball is Ellipse ellipse)
                {
                    var ballAnimation = new BallAppearAnimation(ellipse, GamePresenter.FieldSettings.CellSize* 0.8); 
                        //todo DRY, the CellSize*0.8 was copied from DrawBall method 
                    _animationController.AddAnimation(ballAnimation);
                }
            }
            
            _animationController.Tick();
        }
        
        
        public void StartBallDisappearAnimation(IEnumerable<Shape> balls, Action<Animation>? onAnimationFinished)
        {
            foreach (var ball in balls)
            {
                if (ball is Ellipse ellipse)
                {
                    var ballAnimation = new BallDisappearAnimation(ellipse, GamePresenter.FieldSettings.CellSize* 0.8); 
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
        
        public void FieldCellClick(int x, int y)
        {
            if (GamePresenter.State == GameState.WaitingForSelection && GamePresenter.IsWithinField(x, y))
            {
                try
                {
                    var ball = GamePresenter.SelectBall(x, y);
                    (ball as Ellipse).Stroke = new SolidColorBrush(Color.Parse("Black"));
                    (ball as Ellipse).StrokeThickness = 4;
                }
                catch
                {
                }
            }
            
            if (GamePresenter.State == GameState.BallSelected)
            {
                if (GamePresenter.CanMoveTo(x, y))
                {
                    GamePresenter.StartMakingMove(x, y);
                }
                else if (GamePresenter.IsWithinField(x, y) && GamePresenter.Field[x, y] != null)
                {
                    //select another ball
                    try
                    {
                        var selectedBall = GamePresenter.SelectedBall;
                        
                        var ball = GamePresenter.SelectBall(x, y); //todo duplicate code
                        (ball as Ellipse).Stroke = new SolidColorBrush(Color.Parse("Black"));
                        (ball as Ellipse).StrokeThickness = 4;

                        if (selectedBall != null && selectedBall != GamePresenter.SelectedBall)
                        {
                            (selectedBall as Ellipse).StrokeThickness = 0;
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        
        private (int x, int y) GetRandomCell(bool shouldBeEmpty)
        {
            var rnd = new Random();
            
            var cells = new List<(int x, int y)>();
            
            for (var x = 0; x < GamePresenter.FieldWidth; x++)
            for (var y = 0; y < GamePresenter.FieldHeight; y++)
            {
                if ((shouldBeEmpty && GamePresenter.Field[x, y] == null) ||
                    (!shouldBeEmpty && GamePresenter.Field[x, y] != null))
                {
                    cells.Add((x, y));
                }
            }

            if (!cells.Any())
            {
                throw new Exception("No appropriate cell in the field");
            }

            return cells[rnd.Next(0, cells.Count)];
        }

        public (int x, int y) GetRandomBallCellThatCanMoveTo(int toX, int toY)
        {
            for (var i = 0; i < 1000; i++)
            {
                var ballCell = GetRandomCell(false);
                if (GamePresenter.CanMoveTo(ballCell.x, ballCell.y, toX, toY))
                {
                    return ballCell;
                }
            }
            
            // very improbable, but if such ball not found, just go through every ball
            for (var x = 0; x < GamePresenter.FieldWidth; x++)
            for (var y = 0; y < GamePresenter.FieldHeight; y++)
            {
                if (GamePresenter.CanMoveTo(x, y, toX, toY))
                {
                    return (x, y);
                }
            }

            throw new Exception("No move available");
        }
        
        public (int x, int y) GetRandomEmptyCell()
        {
            return GetRandomCell(true);
        }
    }
}