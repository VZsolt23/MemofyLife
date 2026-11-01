using MemofyLife.Minesweeper.Engine;

namespace MemofyLife.Features.Arcade.Games.Minesweeper;

public static class MinesweeperDifficulty
{
    public const string Easy = "easy";
    public const string Medium = "medium";
    public const string Hard = "hard";

    public static BoardSettings GetSettings(string? variantKey)
    {
        return variantKey switch
        {
            Medium => new BoardSettings(16, 16, 40),
            Hard => new BoardSettings(30, 16, 99),
            _ => new BoardSettings(9, 9, 10),
        };
    }
}
