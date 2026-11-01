using MemofyLife.Minesweeper.Engine.Enums;

namespace MemofyLife.Minesweeper.Engine;

public sealed record MinesweeperSnapshot(
    int SchemaVersion,
    int Rows,
    int Columns,
    int Mines,
    GameStatus Status,
    IReadOnlyList<CellState> Cells,
    IReadOnlyList<int> MineIndexes)
{
    public const int CurrentVersion = 1;
}
