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
using UglyLines.Desktop.ViewModels;
using UglyLines.Logic;

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

        private void GameField_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (MainViewModel.GamePresenter != null && MainViewModel.GamePresenter.State == GameState.GameOver)
            {
                return;
            }
            
            var p = e.GetCurrentPoint(_fieldCanvas);
            int x = (int)Math.Floor((p.Position.X - _fieldSettings.LeftMargin) / _fieldSettings.CellSize);
            int y = (int)Math.Floor((p.Position.Y - _fieldSettings.TopMargin) / _fieldSettings.CellSize);

            MainViewModel.FieldCellClick(x, y);
        }


        private void ReloadButton_OnClick(object? sender, RoutedEventArgs e)
        {
            //todo this might be unnecessary
            var shapesToRemove = new List<Ellipse>();
            
            foreach (var control in _fieldCanvas.Children)
            {
                if (control is Ellipse ellipse)
                {
                    shapesToRemove.Add(ellipse);
                }
            }

            _fieldCanvas.Children.RemoveAll(shapesToRemove);
           
            MainViewModel.StartGame(_fieldCanvas);
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
                MainViewModel.AutoMove();
            }
            catch
            {
                // can't move
            }
        }
    }
}