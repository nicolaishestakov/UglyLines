namespace UglyLines.Logic;
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
        |
     /  or \
    /      V
   / [ClearingLines]    The ball is moved in the field in the new position
   |                Balls in the lines to be cleared are marked (BallsToClear property)
   |                The balls to clear are kept in the field 
   |                Delete lines animation can be performed here
ShootNewBalls()       
   \                           
     \                 
     V     V 
    [ShootingNewBalls] New balls are to be added in the field
                    The balls to clear are removed from the field
                    The balls to be added are marked (BallsToShoot property)
                    Ball appear animation can be done here
            V
        FinishShootingBalls()
            V
    [ClearingLines]
            |        Clearing balls in the lines completed after adding the new balls
            |        Balls in the lines to be cleared are marked (BallsToClear property)
            |        The balls to clear are kept in the field 
            |        Delete lines animation can be performed here
            |        
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

        if (Field.IsWithinBounds(xy) && Field.GetBall(xy) != null)
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

        if (Field.GetBall(from) == null || Field.GetBall(to) != null)
        {
            return false;
        }
        
        var field = new bool[Field.Width, Field.Height];
        
        for (var x = 0; x <= field.GetUpperBound(0); x++)
        for (var y = 0; y <= field.GetUpperBound(1); y++)
        {
            field[x, y] = Field.GetBall(new Location(x, y)) != null;
        }

        var pathFinder = new Pathfinder(field);

        return pathFinder.CanMove((from.X, from.Y), (to.X, to.Y));
    }
    
    public bool CanMove(Location xy)
    {
        var from = SelectedBall?.Location;

        if (from == null) return false;

        return CanMove(from, xy);
    }
    
    public bool StartMove(Location whereToMove)
    {
        ValidateGameState(new[] {GameState.BallSelected});

        if (SelectedBall == null) throw new Exception("Ball is not selected");

        if (!CanMove(whereToMove))
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

        SearchLinesToClear(new[]{to});

        if (_ballsToClear.Any())
        {
            GameState = GameState.ClearingLines;            
        }
        else
        {
            ShootNewBalls();
        }
    }

    private void SearchLinesToClear(IEnumerable<Location> to)
    {
        _ballsToClear.Clear();
        
        foreach (var cellToClear in GetLinesToClear(to))
        {
            var ball = Field.GetBall(cellToClear);

            if (ball != null)
            {
                _ballsToClear.Add(new BallXY(ball, cellToClear));
            }
        }
    }

    public void ShootNewBalls()
    {
        ValidateGameState(new[] { GameState.BallMoving });

        Field.RemoveBalls(_ballsToClear.Select(b => b.Location));
        _ballsToClear.Clear();

        _ballsToShoot.AddRange(_nextMoveBalls.ThrowBalls(_nextBalls, Field.GetEmptyCells()));
        
        GameState = GameState.ShootingNewBalls;
    }

    public void FinishShootingBalls()
    {
        foreach (var ballXy in _ballsToShoot)
        {
            Field.AddBall(ballXy);
        }

        var addedLocations = _ballsToShoot.Select(b => b.Location).ToList();
        _ballsToShoot.Clear();
        
        SearchLinesToClear(addedLocations);

        GameState = GameState.ClearingLines;
    }

    public void NextMoveOrEndGame()
    {
        ValidateGameState(new[] { GameState.ShootingNewBalls, GameState.ClearingLines });
        
        Field.RemoveBalls(_ballsToClear.Select(b => b.Location));
        _ballsToClear.Clear();
        
        _nextBalls.Clear();
        _nextBalls.AddRange(_nextMoveBalls.GetBallsForNextMove());

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


    public void Restart()
    {
        Field.Clear();
        _nextBalls.Clear();
        _ballsToClear.Clear();
        MovingBall = null;
        ChangeSelectedBallCell(null);

        _gameState = GameState.BallMoving;
        
        _nextBalls.AddRange(_nextMoveBalls.GetBallsForNextMove());
        ShootNewBalls();
    }
    
    private IEnumerable<Location> GetLinesToClear(IEnumerable<Location> cellsToCheck) 
    {
        var lineCounter = new LineCounter(new FieldAdapter(Field));
        foreach (var location in cellsToCheck)
        {
            foreach (var result in lineCounter.GetCompleteLines(location))
            {
                yield return result;
            }
        }
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

            var ball = Field.GetBall(_selectedBallCell);

            return ball == null ? null : new BallXY(ball, _selectedBallCell);
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
        
        var ball = Field.GetBall(newSelection);
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

    public Game(int fieldWidth, int fieldHeight, INextMoveBalls nextMoveBalls)
    {
        _nextMoveBalls = nextMoveBalls;
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

    private INextMoveBalls _nextMoveBalls;
}