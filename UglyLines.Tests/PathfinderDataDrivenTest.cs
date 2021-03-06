using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using UglyLines.Logic;
using Xunit;

namespace UglyLines.Tests;

public class PathfinderDataDrivenTest
{
    public PathfinderDataDrivenTest()
    {
        _pathfinder = Pathfinder.Create(_field, i => i != 0);
    }

    private readonly Pathfinder _pathfinder;
    
    private readonly int[,] _field = 
    { 
        //0  1  2  3  4  5  6
        { 1, 0, 0, 1, 0, 0, 0 }, // x == 0
        { 0, 0, 0, 1, 0, 1, 0 }, // x == 1
        { 0, 1, 1, 1, 0, 1, 0 }, // x == 2
        { 0, 1, 0, 0, 0, 1, 1 }  // x == 3
    };

    [Theory]
    [InlineData(1, 0, 0, 2, true)]
    [InlineData(1, 0, 1, 3, true)]  // destination cell is occupied
    [InlineData(1, 0, 3, 4, false)]
    [InlineData(3, 2, 2, 6, true)]
    [InlineData(0, 3, 0, 2, true)]  // starting cell is occupied
    [InlineData(0, 3, 0, 4, true)]
    public void CanMoveTo(int fromX, int fromY, int toX, int toY, bool canMove)
    {
        _pathfinder.CanMove((fromX, fromY), (toX, toY)).Should().Be(canMove);
    }

    [Theory, MemberData(nameof(CellsAvailableFromData))]
    public void CellsAvailableFrom((int X, int Y) from, IEnumerable<(int X, int Y)> availableCellsExpected)
    {
        var availableCells = _pathfinder.CellsAvailableFrom(from).ToList();

        availableCells.Should().BeEquivalentTo(availableCellsExpected);
    }
    
    public static IEnumerable<object[]> CellsAvailableFromData => 
        new List<object[]>
        {
            new object[] { (3, 0), new [] {(2, 0), (1, 0), (1, 1), (1, 2), (0, 1), (0, 2)} },
            new object[] { (3, 6), new [] {(2, 6), (1, 6), (0, 6), (0, 5), (0, 4), (1, 4), (2, 4), (3, 4), (3, 3), (3, 2)} },
            new object[] { (1, 3), new []
            {
                (2, 6), (1, 6), (0, 6), (0, 5), (0, 4), (1, 4), (2, 4), (3, 4), (3, 3), (3, 2),
                (3, 0), (2, 0), (1, 0), (1, 1), (1, 2), (0, 1), (0, 2)
            } }
        };
}