namespace MemofyLife.Minesweeper.Engine;

public interface IMinePlacer
{
    IReadOnlySet<(int Row, int Column)> Place(BoardSettings settings, (int Row, int Column) safeCell);
}
