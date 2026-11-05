using MemofyLife.Minesweeper.Engine;

namespace MemofyLife.Tests.Arcade.Minesweeper;

public class RandomMinePlacerTests
{
    [Fact]
    public void Place_ReturnsExactlyTheRequestedNumberOfMines_AndSkipsTheSafeCell()
    {
        var placer = new RandomMinePlacer(new Random(42));
        var settings = new BoardSettings(4, 4, 15);

        IReadOnlySet<(int Row, int Column)> mines = placer.Place(settings, (2, 2));

        Assert.Equal(15, mines.Count);
        Assert.DoesNotContain((2, 2), mines);
    }

    [Fact]
    public void Place_KeepsEveryMineInsideTheBoard()
    {
        var placer = new RandomMinePlacer(new Random(7));
        var settings = new BoardSettings(5, 3, 10);

        IReadOnlySet<(int Row, int Column)> mines = placer.Place(settings, (0, 0));

        Assert.All(mines, m =>
        {
            Assert.InRange(m.Row, 0, 4);
            Assert.InRange(m.Column, 0, 2);
        });
    }
}
