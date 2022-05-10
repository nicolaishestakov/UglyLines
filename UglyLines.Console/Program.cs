// See https://aka.ms/new-console-template for more information

/*
 * Disclaimer:
 * This is the console prototype, like MVP, but even more simple and dirty code-wise
 * It represents the zero iteration of coding, like to have a proof-of-concept, or trying to make something working quick
 * I do not recommend pushing such code in production even for MVP! But as a sketch, why not.
 * From the functional point of view, it is a business question,
 * whether such restricted and ugly prototype has value for the stakeholders or not
 */

using UglyLines.Logic;

Console.WriteLine("Hello, World!");
var random = new Random();
BallColor[] BallColors = Enum.GetValues<BallColor>();


var game = new Game(9, 9);
game.Restart(
    new[]
    {
        GetRandomBall(random),  //todo extract GetNextBalls method
        GetRandomBall(random),
        GetRandomBall(random)
    });

//Restart shoots the balls and lets the presenter do the animation
//So it is required to finish the shooting and move to next turn
game.FinishShootingBalls();
game.NextMoveOrEndGame(new[]
{
    GetRandomBall(random),
    GetRandomBall(random),
    GetRandomBall(random)
});


while (true)
{
    PrintField(game.Field, game.NextBalls);

    var command = Console.ReadLine();

    if (command == "exit")
    {
        break;
    }

    try
    {
        var move = ParseMoveCommand(command??string.Empty);

        if (move == null)
        {
            throw new Exception("Unrecognized command");
        }

        MakeMove(game, move.Value.from, move.Value.to, random);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }

    if (game.GameState == GameState.GameOver)
    {
        Console.WriteLine("GAME OVER");
        break;
    }
}

Console.ReadLine();


// functions
 
(Location from, Location to)? ParseMoveCommand(string input)
{
    var parts = input.Split(' ');
    if (parts.Length < 2) return null;

    var from = parts[0];
    var to = parts[1];
    
    if (from.Length != 2 || to.Length != 2) return null;

    if (!int.TryParse(from[0].ToString(), out int x1)) return null;
    if (!int.TryParse(from[1].ToString(), out int y1)) return null;
    if (!int.TryParse(to[0].ToString(), out int x2)) return null;
    if (!int.TryParse(to[1].ToString(), out int y2)) return null;

    return (new Location(x1, y1), new Location(x2, y2));
}

void PrintBall(IBall ball)
{
    Console.Write((int)ball.Color);
}

void PrintField(Field field, IEnumerable<IBall> nextBalls)
{
    Console.Write("  012345678   -> "); //todo adjust for different field width

    foreach (var ball in nextBalls)
    {
        PrintBall(ball);
    }
    Console.WriteLine();
    
    for (var y = 0; y < field.Height; y++)
    {
        Console.Write($" {y.ToString()[0]}");
        
        for (var x = 0; x < field.Width; x++)
        {
            var ball = field.GetBall(new Location(x, y));

            if (ball == null)
            {
                Console.Write('.');
            }
            else
            {
                PrintBall(ball);
            }
        }
        Console.WriteLine();
    }
    
    Console.WriteLine();
}

void MakeMove(Game game, Location from, Location to, Random random)
{
    if (!game.SelectBall(from))
    {
        throw new Exception($"Ball at {from.X},{from.Y} cannot be selected");
    }

    game.StartMove(to);
    game.FinishMove();

    if (game.GameState == GameState.ShootingNewBalls)
    {
        game.FinishShootingBalls();
    }

    game.NextMoveOrEndGame(new[]
    {
        GetRandomBall(random),
        GetRandomBall(random),
        GetRandomBall(random)
    });

}




IBall GetRandomBall(Random random)
{
    //todo DRY, similar code in GamePresenter
    var colorIndex = random.Next(BallColors.GetLowerBound(0), BallColors.GetUpperBound(0) +1);

    return new Ball() { Color = BallColors[colorIndex] };
}

class Ball : IBall
{
    public BallColor Color { get; set; }
}