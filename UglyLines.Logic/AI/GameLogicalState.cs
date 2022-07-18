namespace UglyLines.Logic.AI;

internal class GameLogicalState: IGame
{
    public IColorField Field { get; }
    public IEnumerable<BallColor> NextBalls { get; }
    public IEnumerable<Location> GetAvailableMoves(Location @from)
    {
        throw new NotImplementedException();
    }

    public bool IsMovePossible(Location @from, Location to)
    {
        throw new NotImplementedException();
    }
}