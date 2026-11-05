namespace MemofyLife.Minesweeper.Engine;

public sealed class RandomMinePlacer : IMinePlacer
{
    private readonly Random _random;

    public RandomMinePlacer() : this(Random.Shared)
    { }

    public RandomMinePlacer(Random random)
    {
        _random = random;
    }

    public IReadOnlySet<(int Row, int Column)> Place(BoardSettings settings, (int Row, int Column) safeCell)
    {
        var candidates = new List<(int Row, int Col)>(settings.Rows * settings.Columns);

        for (var row = 0; row < settings.Rows; row++)
        {
            for (var col = 0; col < settings.Columns; col++)
            {
                if ((row, col) != safeCell)
                {
                    candidates.Add((row, col));
                }
            }
        }

        var mines = new HashSet<(int Row, int Col)>(settings.Mines);

        for (var i = 0; i < settings.Mines; i++)
        {
            var pick = _random.Next(i, candidates.Count);
            (candidates[i], candidates[pick]) = (candidates[pick], candidates[i]);

            mines.Add(candidates[i]);
        }

        return mines;
    }
}
