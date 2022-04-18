using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using UglyLines.Desktop.Logic;

namespace UglyLines.Desktop.ViewModels;

public class ShapeBall: IBall
{
    public ShapeBall(BallColor color, double cellSize)
    {
        Color = color;
        Shape = CreateShape(cellSize);
    }

    public BallColor Color { get; }
    
    public Shape Shape { get; }

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
            Canvas.SetLeft(Shape, x);
            Canvas.SetTop(Shape, y);
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

    protected virtual Shape CreateShape(double cellSize) 
    {
        var ellipse = new Ellipse()
        {
            Fill = new SolidColorBrush(GetBallShapeColor(Color)),
            Width = cellSize * 0.8,
            Height = cellSize * 0.8,
        };

        return ellipse;
    }

    protected Color GetBallShapeColor(BallColor ballColor)
    {
        return BallColors[ballColor];
    }

    private static readonly Dictionary<BallColor, Color> BallColors = new Dictionary<BallColor, Color>()
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