namespace UglyLines.Logic;

public interface IColorField
{
    BallColor? GetBallColor(Location xy);
    public int Width { get; }
    public int Height { get; }
}

public class LineCounter
{ 
    public LineCounter(IColorField field)
    {
        _field = field;
    }
    
    public IEnumerable<Location> GetCompleteLines(Location searchFrom)
    {
        var color = _field.GetBallColor(searchFrom);
        
        if (color == null)
        {
            throw new Exception("Empty search location");
        }

        var result = new List<Location>();
        
        // check horizontal line
        result.AddRange(GetCompleteLineBallsInDirection(searchFrom, 1, 0, color.Value));
        
        // check vertical line
        result.AddRange(GetCompleteLineBallsInDirection(searchFrom, 0, 1, color.Value));
        
        // check diagonal right-down line
        result.AddRange(GetCompleteLineBallsInDirection(searchFrom, 1, 1, color.Value));
        
        // check diagonal right-up line
        result.AddRange(GetCompleteLineBallsInDirection(searchFrom, 1, -1, color.Value));

        return result;
    }
    
    
    private IColorField _field;
    
    private Location GetLineEndCellInDirection(Location startCell, int dx, int dy, BallColor color)
    {
        var nextX = startCell.X + dx;
        var nextY = startCell.Y + dy;

        while (_field.GetBallColor(new Location(nextX, nextY)) == color)
        {
            nextX += dx;
            nextY += dy;
        }
        
        // nextX, nextY points to the first cell that does not fit
        // return previous cell

        return new Location(nextX - dx, nextY - dy);
    }
    
    private List<Location> GetCompleteLineBallsInDirection(Location startCell, int dx, int dy, BallColor color)
    {
        var lineEnd1 = GetLineEndCellInDirection(startCell, dx, dy, color);
        var lineEnd2 = GetLineEndCellInDirection(startCell, -dx, -dy, color);

        var xLen = Math.Abs(lineEnd1.X - lineEnd2.X) + 1;
        var yLen = Math.Abs(lineEnd1.Y - lineEnd2.Y) + 1;

        var lineLength = Math.Max(xLen, yLen);

        var result = new List<Location>();

        if (lineLength >= 5)
        {
            //complete line
            var cell = lineEnd1;

            do
            {
                result.Add(cell);
                cell = new Location(cell.X - dx, cell.Y - dy);

                if (!IsWithinField(cell))
                {
                    throw new Exception("Logical error while processing lines");
                }
            } while (cell != lineEnd2);
            
            result.Add(cell); //this is lineEnd2
        }

        return result;
    }

    private bool IsWithinField(Location xy)
    {
        return xy.X >= 0 && xy.X < _field.Width && xy.Y >= 0 && xy.Y < _field.Height;
    }
}