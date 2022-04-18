using System;
using System.Collections.Generic;
using System.Linq;

namespace UglyLines.Desktop.Logic;
/*
    [WaitingForSelection] The balls marked to clear are removed from the field
            |    
        SelectBall(xy)
            V
    [BallSelected]  The ball is marked as selected (SelectedBall property)
            |
        StartMove(xy)
            V
    [BallMoving]    The ball is marked as moving, still kept on the old position in the field
                    The ball selection is removed
                    Moving animation can be performed here
            V
        FinishMove()
            V
    [ClearingLines]    The ball is moved in the field in the new position
                    Balls in the lines to be cleared are marked (BallsToClear property)
                    The balls to clear are kept in the field 
                    Delete lines animation can be performed here
            V
        ShootNewBalls()                       
                      
            V 
    [ShootingNewBalls] New balls are to be added in the field
                    The balls to clear are removed from the field
                    The balls to be added are marked (BallsToShoot property)
                    Ball appear animation can be done here
            V
        FinishShootingBalls()
            V
    [ClearingLines]    //TODO not yet implemented
                    Clearing balls in the lines completed after adding the new balls
                    Balls in the lines to be cleared are marked (BallsToClear property)
                    The balls to clear are kept in the field 
                    Delete lines animation can be performed here
                    //TODO May need an additional state to distinguish from ClearLines after FinishMove()
            V
         NextMoveOrEndGame()  If the field is not full, proceed to [WaitingForSelection]
            V
    [GameOver] 

 */



public class Game
{
    public bool SelectBall(Location xy)
    {
        ValidateGameState(new []{GameState.WaitingForSelection, GameState.BallSelected});

        if (Field.GetBall(xy) != null)
        {
            ChangeSelectedBallCell(xy);
            GameState = GameState.BallSelected;
            return true;
        }

        return false;
    }

    public bool CanMove(Location from, Location to)
    {
        if (!Field.IsWithinBounds(from) || !Field.IsWithinBounds(to))
        {
            return false;
        }
        
        //todo check via PathFinder
        return Field.GetBall(from) != null && Field.GetBall(to) == null;
    }
    
    public bool CanMove(Location xy)
    {
        var from = SelectedBall?.Location;

        if (from == null) return false;

        return CanMove(from.Value, xy);
    }
    
    public bool StartMove(Location whereToMove)
    {
        ValidateGameState(new[] {GameState.BallSelected});

        if (SelectedBall == null) throw new Exception("Ball is not selected");

        if (Field.GetBall(whereToMove) != null) //todo check CanMove via PathFinder
        {
            return false;
        }

        var ball = SelectedBall.Ball;
        var from = SelectedBall.Location;

        ChangeSelectedBallCell(null);
        MovingBall = (ball, from, whereToMove);
        GameState = GameState.BallMoving;
        return true;
    }

    public void FinishMove()
    {
        ValidateGameState(new[] { GameState.BallMoving });

        if (MovingBall == null) throw new Exception("Moving ball is not set");

        (_, var from, var to) = MovingBall.Value;
        
        Field.MoveBall(from, to);
        MovingBall = null;

        _ballsToClear.Clear();
        foreach (var cellToClear in GetLinesToClear(to))
        {
            var ball = Field.GetBall(cellToClear);

            if (ball != null)
            {
                _ballsToClear.Add(new BallXY(ball, cellToClear));
            }
        }

        GameState = GameState.ClearingLines;
    }

    public void ShootNewBalls()
    {
        ValidateGameState(new[] { GameState.ClearingLines });

        Field.RemoveBalls(_ballsToClear.Select(b => b.Location));
        _ballsToClear.Clear();

        var emptyCells = Field.GetEmptyCells().ToList();
        
        var rnd = new Random();
        
        foreach (var newBall in _nextBalls)
        {
            if (!emptyCells.Any())
            {
                break;
            }
            
            var cellIndex = rnd.Next(0, emptyCells.Count - 1);
                
            _ballsToShoot.Add(new BallXY(newBall, emptyCells[cellIndex]));
            
            emptyCells.RemoveAt(cellIndex);
        }

        GameState = GameState.ShootingNewBalls;
    }

    public void FinishShootingBalls()
    {
        foreach (var ballXy in _ballsToShoot)
        {
            Field.AddBall(ballXy);
        }
        _ballsToShoot.Clear();
        
        //todo need to implement removing the lines after new balls are added
        // Find balls to clear

        GameState = GameState.ClearingLines;
    }

    public void NextMoveOrEndGame(IEnumerable<IBall> nextBalls)
    {
        ValidateGameState(new[] { GameState.ShootingNewBalls, GameState.ClearingLines });
        
        Field.RemoveBalls(_ballsToClear.Select(b => b.Location));
        _ballsToClear.Clear();
        
        _nextBalls.Clear();
        _nextBalls.AddRange(nextBalls);

        foreach (var ballXy in BallsToShoot)
        {
            Field.AddBall(ballXy);
        }
        
        _ballsToShoot.Clear();
        
        if (!Field.GetEmptyCells().Any())
        {
            GameState = GameState.GameOver;
        }
        else
        {
            GameState = GameState.WaitingForSelection;    
        }
    }


    public void Restart(IEnumerable<IBall> ballsOnField, IEnumerable<IBall> nextBalls)
    {
        Field.Clear();
        _nextBalls.Clear();
        _ballsToClear.Clear();
        MovingBall = null;
        ChangeSelectedBallCell(null);

        _gameState = GameState.ClearingLines;
        
        _nextBalls.AddRange(ballsOnField);
        ShootNewBalls();
    }
    
    private IEnumerable<Location> GetLinesToClear(Location cellToCheck) 
    {
        var lineCounter = new LineCounter(new FieldAdapter(Field));
        return lineCounter.GetCompleteLines(cellToCheck);
    }
    public Field Field { get; private set; }

    private GameState _gameState;
    public GameState GameState
    {
        get => _gameState;
        private set
        {
            if (value != _gameState)
            {
                var oldValue = _gameState;
                _gameState = value;
                GameStateChanged?.Invoke(this, new GameStateChangedEventArgs(){OldState = oldValue, NewState = value});
            }
        }
    }
    public class GameStateChangedEventArgs : EventArgs
    {
        public GameState OldState { get; set; }
        public GameState NewState { get; set; }
    }

    public event EventHandler<GameStateChangedEventArgs>? GameStateChanged;

    
    private Location? _selectedBallCell;

    public BallXY? SelectedBall
    {
        get
        {
            if (_selectedBallCell == null)
            {
                return null;
            }

            var ball = Field.GetBall(_selectedBallCell.Value);

            return ball == null ? null : new BallXY(ball, _selectedBallCell.Value);
        }
    }

    public class SelectedBallChangedEventArgs : EventArgs
    {
        public BallXY? OldSelectedBall { get; set; }
        public BallXY? NewSelectedBall { get; set; }
    }

    public event EventHandler<SelectedBallChangedEventArgs>? SelectedBallChanged;
    private void ChangeSelectedBallCell(Location? newSelection)
    {
        if (_selectedBallCell == newSelection)
        {
            return;
        }

        var oldSelectedBall = SelectedBall;
        
        if (newSelection == null)
        {
            _selectedBallCell = null;
            SelectedBallChanged?.Invoke(this,
                new SelectedBallChangedEventArgs() { OldSelectedBall = oldSelectedBall, NewSelectedBall = null });
            return;
        }
        
        var ball = Field.GetBall(newSelection.Value);
        if (ball == null)
        {
            throw new ArgumentException($"{nameof(newSelection)} location contains no ball");
        }

        _selectedBallCell = newSelection;
        SelectedBallChanged?.Invoke(this,
            new SelectedBallChangedEventArgs() { OldSelectedBall = oldSelectedBall, NewSelectedBall = SelectedBall });
    }

    /// <summary>
    /// The state of the moving ball
    /// </summary>
    public (IBall Ball, Location From, Location To)? MovingBall { get; private set; }


    private readonly List<BallXY> _ballsToClear = new List<BallXY>();
    /// <summary>
    /// The balls with locations being removed from the field
    /// </summary>
    public IEnumerable<BallXY> BallsToClear => _ballsToClear;


    private readonly List<IBall> _nextBalls = new List<IBall>();
    /// <summary>
    /// The balls to be added on the next move
    /// </summary>
    public IEnumerable<IBall> NextBalls => _nextBalls;
    
    
    private readonly List<BallXY> _ballsToShoot = new List<BallXY>();
    /// <summary>
    /// The balls with locations being shut in the field
    /// </summary>
    public IEnumerable<BallXY> BallsToShoot => _ballsToShoot;
    
    private class FieldAdapter : IColorField
    {
        public FieldAdapter(Field field)
        {
            _field = field;
        }

        public BallColor? GetBallColor(Location xy)
        {
            if (!_field.IsWithinBounds(xy))
            {
                return null;
            }
            
            return _field.GetBall(xy)?.Color;
        }

        public int Width => _field.Width;
        public int Height => _field.Height;

        private Field _field;

        
    }

    public Game(int fieldWidth, int fieldHeight)
    {
        Field = new Field(fieldWidth, fieldHeight);
        //_lineCounter = lineCounter;
    }

    private void ValidateGameState(GameState[] expectedStates)
    {
        if (!expectedStates.Contains(_gameState))
        {
            throw new Exception("Invalid operation for the game state");
        }
    }
}