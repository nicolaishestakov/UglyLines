using System;
using FluentAssertions;
using UglyLines.Desktop.Logic;
using Xunit;

namespace UglyLines.Tests;

public class FieldTest
{
    private readonly Field _field = new Field(5, 5);

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(4, 4, true)]
    [InlineData(1, 2, true)]
    [InlineData(0, 3, true)]
    [InlineData(-1, 0, false)]
    [InlineData(-6, -7, false)]
    [InlineData(3, 5, false)]
    [InlineData(5, 5, false)]
    [InlineData(100, 1000, false)]
    [InlineData(100, -100, false)]
    public void AddGetOutOfBounds(int x, int y, bool isWithinField)
    {
        var ball = new BallStub();
        var xy = new Location(x, y);
        
        if (isWithinField)
        {
            _field.GetBall(xy).Should().BeNull();
            _field.AddBall(new BallXY(ball, xy));
            _field.GetBall(xy).Should().Be(ball);
        }
        else
        {
            _field.Invoking(f => f.GetBall(xy)).Should().Throw<ArgumentOutOfRangeException>();
        }
    }
    
    private class BallStub : IBall
    {
        public BallStub()
        {
            Color = BallColor.Blue;
        }

        public BallStub(BallColor color)
        {
            Color = color;
        }
        public BallColor Color { get; set; }
    }
}