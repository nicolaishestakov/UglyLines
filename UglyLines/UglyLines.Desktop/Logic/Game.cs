using System;

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
    [ClearLines]    The ball is moved in the field in the new position
                    Balls in the lines to be cleared are marked (BallsToClear property)
                    The balls to clear are kept in the field 
                    Delete lines animation can be performed here
            V
        ShootNewBalls() Here a new game may be started                       
                      
            V 
    [ShootNewBalls] New balls are to be added in the field
                    The balls to clear are removed from the field
                    The balls to be added are marked (BallsToShoot property)
                    Ball appear animation can be done here
            V
    [ClearLines]    //TODO not yet implemented
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

    public event EventHandler<GameStateChangedEventArgs> GameStateChanged;

    
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

    public event EventHandler<SelectedBallChangedEventArgs> SelectedBallChanged;
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
        
        BallXY? newBallXY = null;
        
        var ball = Field.GetBall(newSelection.Value);
        if (ball == null)
        {
            throw new ArgumentException($"{nameof(newSelection)} location contains no ball");
        }

        _selectedBallCell = newSelection;
        SelectedBallChanged?.Invoke(this,
            new SelectedBallChangedEventArgs() { OldSelectedBall = oldSelectedBall, NewSelectedBall = SelectedBall });
    }
    
}