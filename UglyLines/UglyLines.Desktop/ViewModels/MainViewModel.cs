using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using UglyLines.Desktop.Views;
using UglyLines.Logic;

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
        
        public GamePresenter? GamePresenter { get; private set; }

        private DispatcherTimer _animationTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(100)};
        //todo the timer is Avalonia dependency, some cross-platform abstraction is needed here
        
        private AnimationController _animationController = new ();
        
        public void StartGame(Canvas canvas)
        {
            GamePresenter = new GamePresenter(new FieldSettings
                {
                    LeftMargin = 30,
                    TopMargin = 80,
                    CellSize = 40,
                    Width = 9, //todo DRY
                    Height = 9

                },
                9, 9, //todo DRY 
                _animationController,
                canvas);
            
            GamePresenter.Restart();
        }
        
        public void FieldCellClick(int x, int y)
        {
            if (GamePresenter == null)
            {
                throw new Exception("Not initialized");
            }
            
            if (GamePresenter.State == GameState.WaitingForSelection)
            {
                GamePresenter.SelectBall(x, y);
            }
            else if (GamePresenter.State == GameState.BallSelected)
            {
                if (!GamePresenter.SelectBall(x, y))
                {
                    GamePresenter.StartMakingMove(x, y);
                }
            }
        }

        public void AutoMove()
        {
            var cellTo = GetRandomEmptyCell();
            var cellFrom =GetRandomBallCellThatCanMoveTo(cellTo);
                
            FieldCellClick(cellFrom.X, cellFrom.Y);
            FieldCellClick(cellTo.X, cellTo.Y);
        }
        
        private Logic.Location GetRandomCell(bool shouldBeEmpty)
        {
            if (GamePresenter == null)
            {
                throw new Exception("Not initialized");
            }
            
            var rnd = new Random();
            
            var cells = GamePresenter.Game.Field.GetEmptyCells().ToList();
            
            if (!cells.Any())
            {
                throw new Exception("No appropriate cell in the field");
            }

            return cells[rnd.Next(0, cells.Count)];
        }

        private Logic.Location GetRandomBallCellThatCanMoveTo(Logic.Location to)
        {
            if (GamePresenter == null)
            {
                throw new Exception("Not initialized");
            }
            
            for (var i = 0; i < 1000; i++)
            {
                var ballCell = GetRandomCell(false);
                if (GamePresenter.Game.CanMove(ballCell, to))
                {
                    return ballCell;
                }
            }
            
            // very improbable, but if such ball not found, just go through every ball
            for (var x = 0; x < GamePresenter.FieldWidth; x++)
            for (var y = 0; y < GamePresenter.FieldHeight; y++)
            {
                if (GamePresenter.Game.CanMove(new Logic.Location(x, y), to))
                {
                    return new Logic.Location(x, y);
                }
            }

            throw new Exception("No move available");
        }
        
        public Logic.Location GetRandomEmptyCell()
        {
            return GetRandomCell(true);
        }
    }
}