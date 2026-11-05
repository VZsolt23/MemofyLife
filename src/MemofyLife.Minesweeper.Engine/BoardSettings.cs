namespace MemofyLife.Minesweeper.Engine;

public sealed record BoardSettings
{
    public BoardSettings(int rows, int columns, int mines)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rows, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(columns, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(mines, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(mines, (rows * columns) - 1);

        Rows = rows;
        Columns = columns;
        Mines = mines;
    }

    public int Rows { get; }
    public int Columns { get; }
    public int Mines { get; }
}
