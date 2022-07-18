namespace UglyLines.Logic.AI;

public interface IGame
{
    IColorField Field { get; }
    IEnumerable<BallColor> NextBalls { get; }

    IEnumerable<Location> GetAvailableMoves(Location from);
    bool IsMovePossible(Location from, Location to);
}