using MemofyLife.Minesweeper.Engine.Enums;

namespace MemofyLife.Minesweeper.Engine;

public readonly record struct CellView(CellState State, bool IsMine, int AdjacentMines);
