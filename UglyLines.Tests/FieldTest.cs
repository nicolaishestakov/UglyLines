using System;
using FluentAssertions;
using UglyLines.Logic;
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

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 4)]
    [InlineData(1, 2)]
    [InlineData(0, 3)]
    public void CantPlaceOnOccupiedCell(int x, int y)
    {
        var ball = new BallStub();
        var xy = new Location(x, y);

        _field.GetBall(xy).Should().BeNull();
        _field.AddBall(new BallXY(ball, xy));
        _field.Invoking(f => f.AddBall(new BallXY(new BallStub(), xy))).Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 4)]
    [InlineData(1, 2)]
    [InlineData(0, 3)]
    public void AddBallEvent(int x, int y)
    {
        var ball = new BallStub();
        var xy = new Location(x, y);
        
        using var monitoredField = _field.Monitor();

        _field.BallAdded += (sender, args) =>
        {
            args.BallXy.Ball.Should().Be(ball);
            args.BallXy.Location.Should().BeEquivalentTo(new Location(x, y));
        };
        
        _field.AddBall(new BallXY(ball, xy));

        monitoredField.Should().Raise("BallAdded");
    }
    
    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(4, 4, 0, 0)]
    [InlineData(1, 2, 2, 3)]
    [InlineData(0, 3, 3, 0)]
    public void MoveBallEvent(int fromX, int fromY, int toX, int toY)
    {
        var ball = new BallStub();
        var xy = new Location(fromX, fromY);
        
        using var monitoredField = _field.Monitor();
        
        _field.AddBall(new BallXY(ball, xy));

        _field.BallMoved += (sender, args) =>
        {
            args.Ball.Should().Be(ball);
            args.From.Should().BeEquivalentTo(new Location(fromX, fromY));
            args.To.Should().BeEquivalentTo(new Location(toX, toY));
        };
        
        _field.MoveBall(xy, new Location(toX, toY));
        
        monitoredField.Should().Raise("BallMoved");
    }

    [Fact]
    public void ClearBalls()
    {
        var balls = new[]
        {
            new BallXY(new BallStub(), new Location(0, 0)),
            new BallXY(new BallStub(), new Location(4, 4)),
            new BallXY(new BallStub(), new Location(1, 1)),
            new BallXY(new BallStub(), new Location(0, 3)),
        };

        foreach (var ballXy in balls)
        {
            _field.AddBall(ballXy);
        }

        _field.BallsRemoved += (sender, args) =>
        {
            args.RemovedBalls.Should().BeEquivalentTo(balls);
        };
        using var monitoredField = _field.Monitor();
        
        _field.Clear();
        _field.GetBalls().Should().BeEmpty();
        
        monitoredField.Should().Raise("BallsRemoved");
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