using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using UglyLines.Logic;

namespace UglyLines.Desktop.ViewModels;

public class ShapeBall: IBall
{
    public ShapeBall(BallColor color, double cellSize)
    {
        Color = color;
        _cellSize = cellSize;
        Shape = CreateShape();
    }

    public BallColor Color { get; }
    
    public Shape Shape { get; }

    private double _cellSize;
    
    public void PutToCanvas(Canvas canvas, double x, double y)
    {
        RemoveFromCanvas();

        _canvas = canvas;
        canvas.Children.Add(Shape);
        SetPositionOnCanvas(x, y);
    }

    public void RemoveFromCanvas()
    {
        if (_canvas != null)
        {
            _canvas.Children.Remove(Shape);
            _canvas = null;
        }
    }

    public void SetPositionOnCanvas(double x, double y)
    {
        if (_canvas != null)
        {
            Canvas.SetLeft(Shape, GetAdjustedX(x));
            Canvas.SetTop(Shape, GetAdjustedY(y));
        }
    }

    public void DrawAsSelected()
    {
        if (Shape is Ellipse ellipse)
        {
            ellipse.Stroke = new SolidColorBrush(Avalonia.Media.Color.Parse("Black"));
            ellipse.StrokeThickness = 4;
        }
    }

    public void DrawAsUnselected()
    {
        if (Shape is Ellipse ellipse)
        {
            ellipse.StrokeThickness = 0;
        }
    }

    private Canvas? _canvas;
    
    protected virtual Shape CreateShape() 
    {
        var ellipse = new Ellipse()
        {
            Fill = new SolidColorBrush(GetBallShapeColor(Color)),
            Width = GetShapeSize(),
            Height = GetShapeSize(),
        };

        return ellipse;
    }

    private double GetShapeSize()
    {
        return 0.8 * _cellSize;
    }

    private double GetAdjustedX(double cellLeftX)
    {
        return cellLeftX + (_cellSize - GetShapeSize()) / 2;
    }
    private double GetAdjustedY(double cellLeftY)
    {
        return cellLeftY + (_cellSize - GetShapeSize()) / 2;
    }

    protected Color GetBallShapeColor(BallColor ballColor)
    {
        return BallColors[ballColor];
    }

    private static readonly Dictionary<BallColor, Color> BallColors = new()
    {
        { BallColor.Red, Avalonia.Media.Color.Parse("Red") },
        { BallColor.Blue, Avalonia.Media.Color.Parse("Blue") },
        { BallColor.Brown, Avalonia.Media.Color.Parse("Brown") },
        { BallColor.Cyan, Avalonia.Media.Color.Parse("Cyan") },
        { BallColor.Green, Avalonia.Media.Color.Parse("Green") },
        { BallColor.Magenta, Avalonia.Media.Color.Parse("Magenta") },
        { BallColor.Yellow, Avalonia.Media.Color.Parse("Yellow") },
    };
}