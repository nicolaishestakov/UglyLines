namespace UglyLines.Logic.AI;

public interface IPlayer
{
    (Location From, Location To) Move(IGame game);
}