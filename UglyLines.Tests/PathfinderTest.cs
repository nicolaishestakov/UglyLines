using UglyLines.Desktop.Logic;
using Xunit;

namespace UglyLines.Tests;

public class PathfinderTest
{
    [Fact]
    public void SimpleCanMoveTo()
    {
        var field = new int[,]
        {//y= 0  1  2
            { 1, 1, 1 }, // x == 0
            { 0, 1, 0 }, // x == 1
            { 0, 1, 1 }, // x == 2
            { 0, 1, 0 }  // x == 3
        };
        var pathfinder = Pathfinder.Create(field, i => i != 0);

        var canMove = pathfinder.CanMove((1, 0), (3, 0));
        Assert.True(canMove);
        
        // an alternative way using FluentAssertions
        //pathfinder.CanMove((1, 0), (3, 0)).Should().BeTrue();

        canMove = pathfinder.CanMove((1, 0), (3, 2));
        Assert.False(canMove);
        
        // an alternative way using FluentAssertions
        //pathfinder.CanMove((1, 0), (3, 2)).Should().BeFalse();
    }
}