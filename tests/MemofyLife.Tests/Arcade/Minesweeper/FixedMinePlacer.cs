using MemofyLife.Minesweeper.Engine;

namespace MemofyLife.Tests.Arcade.Minesweeper;

internal sealed class FixedMinePlacer : IMinePlacer
{
    private readonly HashSet<(int Row, int Column)> _mines;

    public FixedMinePlacer(params (int Row, int Column)[] mines)
    {
        _mines = [.. mines];
    }

    public IReadOnlySet<(int Row, int Column)> Place(BoardSettings settings, (int Row, int Column) safeCell) => _mines;
}
