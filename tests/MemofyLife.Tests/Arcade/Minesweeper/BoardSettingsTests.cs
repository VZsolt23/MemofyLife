using MemofyLife.Minesweeper.Engine;

namespace MemofyLife.Tests.Arcade.Minesweeper;

public class BoardSettingsTests
{
    [Theory]
    [InlineData(0, 3, 0)]
    [InlineData(3, 0, 0)]
    [InlineData(3, 3, -1)]
    [InlineData(3, 3, 9)]
    public void Constructor_WithInvalidValues_Throws(int rows, int columns, int mines)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoardSettings(rows, columns, mines));
    }

    [Fact]
    public void Constructor_AllowsEveryCellExceptOneToBeAMine()
    {
        var settings = new BoardSettings(3, 3, 8);

        Assert.Equal(8, settings.Mines);
    }
}
