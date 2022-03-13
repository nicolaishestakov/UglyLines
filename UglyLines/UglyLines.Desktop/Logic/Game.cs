using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using UglyLines.Desktop.Views;

namespace UglyLines.Desktop.Logic;

public class Game
{
    public Game(FieldSettings fieldSettings, int fieldWidth, int fieldHeight)
    {
        FieldSettings = fieldSettings;
        FieldWidth = fieldWidth;
        FieldHeight = fieldHeight;
        
        _field = new Shape? [FieldWidth, FieldHeight];
    }
    
    public int FieldWidth { get; }
    public int FieldHeight { get; }

    public Shape?[,] Field => _field;

    public GameState State { get; private set; } = GameState.WaitingForSelection;
    
    public bool IsWithinField(int x, int y) => x >= 0 && x < FieldWidth && y >= 0 && y < FieldHeight;

    public bool CanMoveTo(int fromX, int fromY, int toX, int toY)
    {
        if (!IsWithinField(fromX, fromY) || !IsWithinField(toX, toY))
        {
            return false;
        }
        
        if (_field[toX, toY] != null)
        {
            return false;
        }
        
        var pathfinder = Pathfinder.Create(_field, (shape) => shape != null);

        return pathfinder.CanMove((fromX, fromY), (toX, toY));
    }
    
    public bool CanMoveTo(int x, int y)
    {
        return SelectedBallCell.HasValue && CanMoveTo(SelectedBallCell.Value.x, SelectedBallCell.Value.y, x, y);
    }
    
    
    
    
    private Shape?[,] _field;
    public FieldSettings FieldSettings { get; }


    public Shape? SelectedBall =>
        SelectedBallCell != null ? _field[SelectedBallCell.Value.x, SelectedBallCell.Value.y] : null;


    private readonly List<(int x, int y)> _ballsToClear = new List<(int x, int y)>();
    public IReadOnlyList<(int x, int y)> BallsToClear => _ballsToClear;
    
    public (int x, int y)? SelectedBallCell { get; private set; }

    public Shape? MovingBall { get; private set; }
    public (int x, int y)? MovingBallDestination { get; private set; }

    private readonly List<(int x, int y, Shape ball)> _ballsToShoot = new List<(int x, int y, Shape ball)>();
    public IReadOnlyList<(int x, int y, Shape ball)> BallsToShoot => _ballsToShoot;
    
    
    public Shape SelectBall(int x, int y)
    {
        if (!IsWithinField(x, y))
        {
            throw new ArgumentOutOfRangeException();
        }

        var ball = _field[x, y]; 
        
        if (ball == null)
        {
            throw new ArgumentException();
        }
        
        SelectedBallCell = (x, y);
        State = GameState.BallSelected;
        return ball;
    }

    public bool StartMakingMove(int x, int y)
    {
        if (State != GameState.BallSelected || SelectedBall == null || !SelectedBallCell.HasValue)
        {
            return false;
        }

        if (!CanMoveTo(x, y))
        {
            return false;
        }

        MovingBall = SelectedBall;
        MovingBallDestination = (x, y);
        
        _field[SelectedBallCell.Value.x, SelectedBallCell.Value.y] = null;

        State = GameState.BallMoving;
        return true;
    }

    public bool EndMakingMove()
    {
        if (State != GameState.BallMoving || MovingBall == null || !MovingBallDestination.HasValue)
        {
            return false;
        }

        var ballDest = MovingBallDestination.Value;
        
        _field[ballDest.x, ballDest.y] = MovingBall;
        MovingBall = null;
        MovingBallDestination = null;

        var ballsToClear = CheckBallsToClear(ballDest.x, ballDest.y, _field[ballDest.x, ballDest.y]);
        _ballsToClear.Clear();
        _ballsToClear.AddRange(ballsToClear);
        
        State = GameState.ClearLines;
        return true;
    }

    public bool ClearLinesAndPrepareNewBallsToShoot(IEnumerable<Shape> newBalls)
    {
        foreach (var ballXY in BallsToClear)
        {
            _field[ballXY.x, ballXY.y] = null;
        }

        bool linesDeleted = _ballsToClear.Any(); 
        
        _ballsToClear.Clear();

        if (linesDeleted)
        {
            // when there are lines completed, new balls are not thrown
            State = GameState.WaitingForSelection;
            return true;
        }
        
        var emptyCells = new List<(int x, int y)>();

        for (var x = 0; x < FieldSettings.Width; x++)
        for (var y = 0; y < FieldSettings.Height; y++)
        {
            if (_field[x, y] == null)
            {
                emptyCells.Add((x, y));
            }
        }

        if (!emptyCells.Any())
        {
            return false;
        }

        _ballsToShoot.Clear();
        
        var rnd = new Random();
        
        foreach (var newBall in newBalls)
        {
            var cellIndex = rnd.Next(0, emptyCells.Count - 1);
                
            _ballsToShoot.Add((emptyCells[cellIndex].x,emptyCells[cellIndex].y, newBall));
            
            emptyCells.RemoveAt(cellIndex);

            if (!emptyCells.Any())
                break;
        }

        State = GameState.ShootNewBalls;
        return true;
    }

    public bool ApplyNewBallsAndProceedToNewMoveOrEndGame()
    {
        if (State != GameState.ShootNewBalls)
        {
            return true;
        }
        
        foreach (var newBall in BallsToShoot)
        {
            _field[newBall.x, newBall.y] = newBall.ball;
        }
        
        //todo clear lines if they appear after adding the balls
        
        _ballsToShoot.Clear();

        bool noEmptyCells = true;
        
        for (var x = 0; x < FieldSettings.Width; x++)
        for (var y = 0; y < FieldSettings.Height; y++)
        {
            if (_field[x, y] == null)
            {
                noEmptyCells = false;
                goto EndOfFor;
            }
        }
        
        EndOfFor:
        if (noEmptyCells)
        {
            State = GameState.GameOver;
            return true;
        }

        State = GameState.WaitingForSelection;
        return true;
    }

    private Color? GetCellBallColor(int x, int y)
    {
        if (!IsWithinField(x, y))
        {
            return null;
        }
        
        var brush = _field[x, y]?.Fill as SolidColorBrush;

        return brush?.Color;
    }
    
    private (int x, int y) GetLineEndCellInDirection((int x, int y) startCell, int dx, int dy, Color color)
    {
        var nextX = startCell.x + dx;
        var nextY = startCell.y + dy;

        while (GetCellBallColor(nextX, nextY) == color)
        {
            nextX += dx;
            nextY += dy;
        }
        
        // nextX, nextY points to the first cell that does not fit
        // return previous cell

        return (nextX - dx, nextY - dy);
    }
   
    
    private List<(int x, int y)> GetCompleteLineBallsInDirection((int x, int y) startCell, int dx, int dy, Color color)
    {
        var lineEnd1 = GetLineEndCellInDirection(startCell, dx, dy, color);
        var lineEnd2 = GetLineEndCellInDirection(startCell, -dx, -dy, color);

        var xLen = Math.Abs(lineEnd1.x - lineEnd2.x) + 1;
        var yLen = Math.Abs(lineEnd1.y - lineEnd2.y) + 1;

        var lineLength = Math.Max(xLen, yLen);

        var result = new List<(int x, int y)>();

        if (lineLength >= 5)
        {
            //complete line
            var cell = lineEnd1;

            do
            {
                result.Add(cell);
                cell = (cell.x - dx, cell.y - dy);

                if (!IsWithinField(cell.x, cell.y))
                {
                    throw new Exception("Logical error while processing lines");
                }
            } while (cell != lineEnd2);
            
            result.Add(cell); //this is lineEnd2
        }

        return result;
    }
    
    private List<(int x, int y)> CheckBallsToClear(int x, int y, Shape ballToSet)
    {
        var ballBrush = ballToSet.Fill as SolidColorBrush;
        if (ballBrush == null)
        {
            throw new Exception("Bad ball");
        }

        var result = new List<(int x, int y)>();
        
        var color = ballBrush.Color;
     
        // check horizontal line
        result.AddRange(GetCompleteLineBallsInDirection((x, y), 1, 0, color));
        
        // check vertical line
        result.AddRange(GetCompleteLineBallsInDirection((x, y), 0, 1, color));
        
        // check diagonal right-down line
        result.AddRange(GetCompleteLineBallsInDirection((x, y), 1, 1, color));
        
        // check diagonal right-up line
        result.AddRange(GetCompleteLineBallsInDirection((x, y), 1, -1, color));

        return result;
    }

    private static readonly Color[] _ballColors = new[]
    {
        Color.Parse("Red"),
        Color.Parse("Green"),
        Color.Parse("Blue"),
        Color.Parse("Yellow"),
        Color.Parse("Pink"),
        Color.Parse("LightBlue"),
        Color.Parse("Brown")
    };
    
    public Color GetNextBallColor()
    {
        var random = new Random();
        var colorIndex = random.Next(_ballColors.GetLowerBound(0), _ballColors.GetUpperBound(0) + 1);

        return _ballColors[colorIndex];
    }
}