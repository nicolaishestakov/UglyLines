using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using UglyLines.Desktop.Logic;
using UglyLines.Desktop.ViewModels;

namespace UglyLines.Desktop.Views
{
    public class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            _fieldCanvas = this.FindControl<Canvas>("GameField"); 
            
            DrawFieldGrid();
        }


        private FieldSettings _fieldSettings = new()
        {
            LeftMargin = 30,
            TopMargin = 80,
            CellSize = 40,
            Width = 9,
            Height = 9  //todo DRY
        };

        private Canvas _fieldCanvas;
        private void DrawFieldGrid()
        {
            _fieldCanvas.Background = new SolidColorBrush(Color.FromRgb(190, 190, 190));
            
            var lineColor = Color.FromRgb(120, 120, 120);
            
            var width = _fieldSettings.Width;
            var height = _fieldSettings.Height;

            var tileWidth = _fieldSettings.CellSize;

            var startX = _fieldSettings.LeftMargin;
            var startY = _fieldSettings.TopMargin;

            for (int logicalX = 0; logicalX <= width; logicalX++)
            {
                var x = logicalX * tileWidth + startX;
                var y1 = startY;
                var y2 = startY + height * tileWidth;
                
                bool isEdge = logicalX == 0 || logicalX == width;
                
                var line = new Line()
                {
                    Stroke = new SolidColorBrush(lineColor), 
                    StrokeThickness = isEdge? 5: 2,
                    StartPoint = new Point(x, y1),
                    EndPoint = new Point(x, y2)
                };

                _fieldCanvas.Children.Add(line);
            }

            for (int logicalY = 0; logicalY <= height; logicalY++)
            {
                var x1 = startX;
                var x2 = startX + tileWidth * width;
                var y = startY + logicalY * tileWidth;

                bool isEdge = logicalY == 0 || logicalY == height;
                
                var line = new Line()
                {
                    Stroke = new SolidColorBrush(lineColor), 
                    StrokeThickness = isEdge? 5: 2,
                    StartPoint = new Point(x1, y),
                    EndPoint = new Point(x2, y)
                };

                _fieldCanvas.Children.Add(line);
            }
        }


        private Shape DrawBall(int x, int y, Color color)
        {
            var screenX = _fieldSettings.LeftMargin + _fieldSettings.CellSize * x;
            var screenY = _fieldSettings.TopMargin + _fieldSettings.CellSize * y;

            var ellipse = new Ellipse()
            {
                Fill = new SolidColorBrush(color),
                Width = _fieldSettings.CellSize * 0.8,
                Height = _fieldSettings.CellSize * 0.8,
            };
            
            _fieldCanvas.Children.Add(ellipse);
            
            Canvas.SetLeft(ellipse, screenX + 4);
            Canvas.SetTop(ellipse, screenY + 4);

            return ellipse;
        }

        private MainViewModel MainViewModel
        {
            get
            {
                var vm = DataContext as MainViewModel;
                if (vm == null) throw new NullReferenceException("Empty main view model");

                return vm;
            }
        }

        private void OnFieldClick(int x, int y)
        {
            MainViewModel.FieldCellClick(x, y);

            var game = MainViewModel.Game; 
            
            if (game.State == GameState.BallMoving)
            {
                game.MovingBall.StrokeThickness = 0; //remove selection
                //todo implement animation in UI
                game.EndMakingMove();
            }

            if (game.State == GameState.ClearLines)
            {
                var b1 = DrawBall(0, 0, game.GetNextBallColor()); //todo note that DrawBall needs coordinates
                var b2 = DrawBall(0, 0, game.GetNextBallColor()); // though the actual coordinates will be set by game logic later
                var b3 = DrawBall(0, 0, game.GetNextBallColor()); // if Game for some reason does not do that, the ball shape will
                                                                           // be drawn in the top-left corner, while logically it
                                                                           // will not exist in the field

                var shapesToRemove = game.BallsToClear.Select(b => game.Field[b.x, b.y]).ToList();
                
                game.ClearLinesAndPrepareNewBallsToShoot(new[] { b1, b2, b3 });

                // Not all of the balls b1, b2, b3 might have been added because of completed lines or no space
                // check which balls are in the BallsToShoot and remove unused
                if (!game.BallsToShoot.Any(b => b.ball == b1))
                {
                    _fieldCanvas.Children.Remove(b1);
                }
                if (!game.BallsToShoot.Any(b => b.ball == b2))
                {
                    _fieldCanvas.Children.Remove(b2);
                }
                if (!game.BallsToShoot.Any(b => b.ball == b3))
                {
                    _fieldCanvas.Children.Remove(b3);
                }
                //todo the code above is a clear code smell.
                //  1) it looks like it's copy-pasted 3 times for 3 variables (minor issue)
                //  2) the logic of adding balls is scattered, it has 2 disjoint parts in this method,
                //     and there is an assumption about what should be happening in ClearLinesAndPrepareNewBallsToShoot
                
                
                if (game.State == GameState.ShootNewBalls)
                {
                    MainViewModel.StartBallAppearAnimation(game.BallsToShoot.Select(b => b.ball));
                    
                    game.ApplyNewBallsAndProceedToNewMoveOrEndGame();
                }

                foreach (var shape in shapesToRemove)
                {
                    if (shape != null)
                    {
                        _fieldCanvas.Children.Remove(shape);
                    }
                }
                
                UpdateBallPositions();
            }
        }
        
        private void GameField_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (MainViewModel.Game.State == GameState.GameOver)
            {
                return;
            }
            
            var p = e.GetCurrentPoint(_fieldCanvas);
            int x = (int)Math.Floor((p.Position.X - _fieldSettings.LeftMargin) / _fieldSettings.CellSize);
            int y = (int)Math.Floor((p.Position.Y - _fieldSettings.TopMargin) / _fieldSettings.CellSize);

            OnFieldClick(x, y);
        }


        private void ReloadButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var shapesToRemove = new List<Ellipse>();
            
            foreach (var control in _fieldCanvas.Children)
            {
                if (control is Ellipse ellipse)
                {
                    shapesToRemove.Add(ellipse);
                }
            }

            _fieldCanvas.Children.RemoveAll(shapesToRemove);
            
            //var game = MainViewModel.Game;
            
            MainViewModel.StartGame();

            var game = MainViewModel.Game;

            var b1 = DrawBall(0, 0, game.GetNextBallColor()); //todo DRY
            var b2 = DrawBall(0, 0, game.GetNextBallColor());
            var b3 = DrawBall(0, 0, game.GetNextBallColor());

            game.ClearLinesAndPrepareNewBallsToShoot(new[] { b1, b2, b3 });
            game.ApplyNewBallsAndProceedToNewMoveOrEndGame();

            UpdateBallPositions();
        }

        private void UpdateBallPositions()
        {
            var game = MainViewModel.Game;
            
            for (var x  = 0; x<=game.Field.GetUpperBound(0); x++)
            for (var y = 0; y <= game.Field.GetUpperBound(1); y++)
            {
                var shape = game.Field[x, y]; 
                if (shape != null)
                {
                    var screenX = _fieldSettings.LeftMargin + _fieldSettings.CellSize * x + 4;
                    var screenY = _fieldSettings.TopMargin + _fieldSettings.CellSize * y + 4;
                    
                    Canvas.SetLeft(shape, screenX);
                    Canvas.SetTop(shape, screenY);
                }
            }
        }


        private void StyledElement_OnDataContextChanged(object? sender, EventArgs e)
        {
            //initial startup
            ReloadButton_OnClick(sender, new RoutedEventArgs());
        }

        private void AutoMoveButton_OnClick(object? sender, RoutedEventArgs e)
        {
            try
            {
                var cellTo = MainViewModel.GetRandomEmptyCell();
                var cellFrom = MainViewModel.GetRandomBallCellThatCanMoveTo(cellTo.x, cellTo.y);
                
                OnFieldClick(cellFrom.x, cellFrom.y);
                OnFieldClick(cellTo.x, cellTo.y);
            }
            catch (Exception exception)
            {
                // can't move
            }
        }
    }
}