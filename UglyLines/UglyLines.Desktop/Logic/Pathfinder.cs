using System;
using System.Collections.Generic;
using System.Linq;

namespace UglyLines.Desktop.Logic;

public class Pathfinder
{
    public static Pathfinder Create<T>(T[,] field, Func<T, bool> isOccupiedFunc)
    {
        var width = field.GetLength(0);
        var height = field.GetLength(1);

        var boolField = new bool[width, height];
        
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            boolField[x, y] = isOccupiedFunc(field[x, y]);
        }

        return new Pathfinder(boolField);
    }
    
    public Pathfinder(bool[,] field)
    {
        _field = field;
    }

    public bool CanMove((int x, int y) from, (int x, int y) to)
    {
        var w = _field.GetLength(0);
        var h = _field.GetLength(1);

        if (!IsWithinBounds(from, w, h) ||
            !IsWithinBounds(to, w, h))
        {
            return false;
        }

        var waveField = GetWaveField(_field);
        
        waveField[from.x, from.y] = 0;

        var waveFront = new List<(int x, int y)>() { from };
        var nextWaveFront = new List<(int x, int y)>();

        while (waveFront.Any())
        {
            foreach (var cell in waveFront)
            {
                var cellWeight = waveField[cell.x, cell.y];

                foreach (var adjCell in GetAdjacentCells(cell))
                {
                    if (adjCell == to)
                    {
                        return true;
                    }
                    
                    var weight = waveField[adjCell.x, adjCell.y];
                    if (cellWeight + 1 < weight)
                    {
                        waveField[adjCell.x, adjCell.y] = cellWeight + 1;
                        nextWaveFront.Add(adjCell);
                    }
                }
            }
            
            waveFront.Clear();
            (waveFront, nextWaveFront) = (nextWaveFront, waveFront);
        }

        return false;
    }

    public IEnumerable<(int X, int Y)> CellsAvailableFrom((int x, int y) from)
    {
        var w = _field.GetLength(0);
        var h = _field.GetLength(1);

        if (!IsWithinBounds(from, w, h))
        {
            yield break;
        }
        
        var waveField = GetWaveField(_field);
        
        
        //todo DRY principle violation
        // this code is copy-pasted from CanMove method with only minor adjustments
        // duplicate logic must be extracted as common code
        
        var waveFront = new List<(int x, int y)>() { from };
        var nextWaveFront = new List<(int x, int y)>();

        while (waveFront.Any())
        {
            foreach (var cell in waveFront)
            {
                var cellWeight = waveField[cell.x, cell.y];

                foreach (var adjCell in GetAdjacentCells(cell))
                {
                    var weight = waveField[adjCell.x, adjCell.y];
                    if (cellWeight + 1 < weight)
                    {
                        waveField[adjCell.x, adjCell.y] = cellWeight + 1;
                        nextWaveFront.Add(adjCell);
                    }
                }
            }
            
            waveFront.Clear();
            (waveFront, nextWaveFront) = (nextWaveFront, waveFront);
        }
        
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
        {
            if (x == from.x && y == from.y)
            {
                continue; // the initial cell is not included as available
            }
            
            var cellWeight = waveField[x, y];

            if (cellWeight > 0 && cellWeight < int.MaxValue)
            {
                yield return (x, y);
            }
        }
    }

    private static int[,] GetWaveField(bool[,] field)
    {
        var w = field.GetLength(0);
        var h = field.GetLength(1);
        
        var waveField = new int[w, h];
        
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
        {
            if (field[x, y]) 
            {
                //can't go here
                waveField[x, y] = -1;
            }
            else
            {
                waveField[x, y] = int.MaxValue;
            }
        }

        return waveField;
    }
    
    private static bool IsWithinBounds((int x, int y) xy, int w, int h)
    {
        return xy.x >= 0 && xy.x < w && xy.y >= 0 && xy.y < h;
    }

    private bool IsWithinBounds((int x, int y) xy)
    {
        return IsWithinBounds(xy, Width, Height);
    }

    private IEnumerable<(int x, int y)> GetAdjacentCells((int x, int y) cell)
    {
        var left = (cell.x - 1, cell.y);
        if (IsWithinBounds(left)) yield return left;

        var upper = (cell.x, cell.y - 1);
        if (IsWithinBounds(upper)) yield return upper;
        
        var right = (cell.x + 1, cell.y);
        if (IsWithinBounds(right)) yield return right;
        
        var lower = (cell.x, cell.y + 1);
        if (IsWithinBounds(lower)) yield return lower;
    }
    
    
    private readonly bool[,] _field;
    private int Width => _field.GetLength(0);
    private int Height => _field.GetLength(1);
}