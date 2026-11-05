using MemofyLife.Features.Arcade.Shared;
using MemofyLife.Navigation;

namespace MemofyLife.Features.Arcade.Games.Minesweeper;

public sealed class MinesweeperGame : IGame
{
    public string Key => "minesweeper";
    public string DisplayName => "Aknakereső";
    public string Icon => "gamepad_solid_full.png";
    public string Route => Routes.MinesweeperPage;
    public BestScoreKind BestScoreKind => BestScoreKind.LowerIsBetter;

    public IReadOnlyList<GameVariant> Variants { get; } =
    [
        new(MinesweeperDifficulty.Easy, "Könnyű"),
        new(MinesweeperDifficulty.Medium, "Közepes"),
        new(MinesweeperDifficulty.Hard, "Nehéz"),
    ];
}
