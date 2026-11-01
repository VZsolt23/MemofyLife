using MemofyLife.Minesweeper.Engine.Enums;

namespace MemofyLife.Minesweeper.Engine;

public sealed class MinesweeperBoard
{
    private readonly BoardSettings _settings;
    private readonly IMinePlacer _minePlacer;
    private readonly CellState[,] _states;
    private readonly bool[,] _mines;
    private readonly int[,] _adjacent;
    private bool _minesPlaced;
    private int _revealedCount;
    private int _flagsPlaced;

    public MinesweeperBoard(BoardSettings settings, IMinePlacer minePlacer)
    {
        _settings = settings;
        _minePlacer = minePlacer;
        _states = new CellState[settings.Rows, settings.Columns];
        _mines = new bool[settings.Rows, settings.Columns];
        _adjacent = new int[settings.Rows, settings.Columns];
        Status = GameStatus.NotStarted;
    }

    public GameStatus Status { get; private set; }

    public int FlagsLeft => _settings.Mines - _flagsPlaced;

    public CellView this[int row, int col] {
        get {
            EnsureInside(row, col);

            CellState state = _states[row, col];
            var exposeMine = state == CellState.Revealed || Status == GameStatus.Lost;

            return new CellView(
                state,
                exposeMine && _mines[row, col],
                state == CellState.Revealed ? _adjacent[row, col] : 0);
        }
    }

    private bool IsInside(int row, int col)
    {
        return row >= 0 && row < _settings.Rows && col >= 0 && col < _settings.Columns;
    }

    private void EnsureInside(int row, int col)
    {
        if (!IsInside(row, col))
        {
            throw new ArgumentOutOfRangeException(nameof(row), $"({row}, {col}) is outside the board.");
        }
    }

    public void Reveal(int row, int col)
    {
        EnsureInside(row, col);

        if (Status is GameStatus.Won or GameStatus.Lost)
        {
            return;
        }

        if (_states[row, col] != CellState.Hidden)
        {
            return;
        }

        if (Status == GameStatus.NotStarted)
        {
            PlaceMines((row, col));
            Status = GameStatus.Playing;
        }

        if (_mines[row, col])
        {
            _states[row, col] = CellState.Revealed;
            Status = GameStatus.Lost;
            return;
        }

        RevealFrom(row, col);

        if (_revealedCount == (_settings.Rows * _settings.Columns) - _settings.Mines)
        {
            Status = GameStatus.Won;
        }
    }

    public void ToggleFlag(int row, int col)
    {
        EnsureInside(row, col);

        if (Status is GameStatus.Won or GameStatus.Lost)
        {
            return;
        }

        if (_states[row, col] == CellState.Hidden)
        {
            _states[row, col] = CellState.Flagged;
            _flagsPlaced++;
        }
        else if (_states[row, col] == CellState.Flagged)
        {
            _states[row, col] = CellState.Hidden;
            _flagsPlaced--;
        }
    }

    public MinesweeperSnapshot Snapshot()
    {
        var cells = new CellState[_settings.Rows * _settings.Columns];
        var mineIndexes = new List<int>(_settings.Mines);

        for (var row = 0; row < _settings.Rows; row++)
        {
            for (var col = 0; col < _settings.Columns; col++)
            {
                var index = (row * _settings.Columns) + col;
                cells[index] = _states[row, col];

                if (_mines[row, col])
                {
                    mineIndexes.Add(index);
                }
            }
        }

        return new MinesweeperSnapshot(
            MinesweeperSnapshot.CurrentVersion,
            _settings.Rows,
            _settings.Columns,
            _settings.Mines,
            Status,
            cells,
            mineIndexes);
    }

    public static MinesweeperBoard Restore(MinesweeperSnapshot snapshot, IMinePlacer minePlacer)
    {
        if (snapshot.SchemaVersion != MinesweeperSnapshot.CurrentVersion)
        {
            throw new NotSupportedException($"Unsupported snapshot version: {snapshot.SchemaVersion}.");
        }

        var settings = new BoardSettings(snapshot.Rows, snapshot.Columns, snapshot.Mines);
        ValidateSnapshot(snapshot, settings);

        var board = new MinesweeperBoard(settings, minePlacer);
        board.LoadState(snapshot);

        return board;
    }

    private void PlaceMines((int Row, int Col) safeCell)
    {
        IReadOnlySet<(int Row, int Col)> mines = _minePlacer.Place(_settings, safeCell);

        if (mines.Count != _settings.Mines || mines.Contains(safeCell))
        {
            throw new InvalidOperationException("The mine placer returned an invalid layout.");
        }

        ApplyMines(mines);
    }

    private void ApplyMines(IEnumerable<(int Row, int Col)> mines)
    {
        foreach ((var row, var col) in mines)
        {
            _mines[row, col] = true;

            foreach ((var r, var c) in GetNeighbors(row, col))
            {
                _adjacent[r, c]++;
            }
        }
    }

    private void RevealFrom(int row, int col)
    {
        var queue = new Queue<(int Row, int Col)>();

        _states[row, col] = CellState.Revealed;
        _revealedCount++;
        queue.Enqueue((row, col));

        while (queue.Count > 0)
        {
            (var r, var c) = queue.Dequeue();

            if (_adjacent[r, c] != 0)
            {
                continue;
            }

            foreach ((var nr, var nc) in GetNeighbors(r, c))
            {
                if (_states[nr, nc] != CellState.Hidden)
                {
                    continue;
                }

                _states[nr, nc] = CellState.Revealed;
                _revealedCount++;
                queue.Enqueue((nr, nc));
            }
        }
    }

    private IEnumerable<(int Row, int Col)> GetNeighbors(int row, int col)
    {
        for (var dr = -1; dr <= 1; dr++)
        {
            for (var dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0)
                {
                    continue;
                }

                var r = row + dr;
                var c = col + dc;

                if (IsInside(r, c))
                {
                    yield return (r, c);
                }
            }
        }
    }

    private static void ValidateSnapshot(MinesweeperSnapshot snapshot, BoardSettings settings)
    {
        var total = settings.Rows * settings.Columns;

        if (snapshot.Cells.Count != total)
        {
            throw new InvalidDataException("The number of cells does not match the board size.");
        }

        if (snapshot.Cells.Any(state => !Enum.IsDefined(state)) || !Enum.IsDefined(snapshot.Status))
        {
            throw new InvalidDataException("The snapshot contains an unknown state value.");
        }

        var expectedMines = snapshot.Status == GameStatus.NotStarted ? 0 : settings.Mines;

        if (snapshot.MineIndexes.Count != expectedMines
            || snapshot.MineIndexes.Distinct().Count() != expectedMines
            || snapshot.MineIndexes.Any(index => index < 0 || index >= total))
        {
            throw new InvalidDataException("The mine positions are invalid.");
        }

        if (snapshot.Status != GameStatus.Lost
            && snapshot.MineIndexes.Any(index => snapshot.Cells[index] == CellState.Revealed))
        {
            throw new InvalidDataException("A revealed mine is only valid after a loss.");
        }

        if (snapshot.Status == GameStatus.NotStarted && snapshot.Cells.Any(state => state == CellState.Revealed))
        {
            throw new InvalidDataException("A game that has not started cannot have revealed cells.");
        }
    }

    private void LoadState(MinesweeperSnapshot snapshot)
    {
        ApplyMines(snapshot.MineIndexes.Select(index => (index / _settings.Columns, index % _settings.Columns)));

        for (var index = 0; index < snapshot.Cells.Count; index++)
        {
            var row = index / _settings.Columns;
            var col = index % _settings.Columns;
            CellState state = snapshot.Cells[index];

            _states[row, col] = state;

            if (state == CellState.Revealed && !_mines[row, col])
            {
                _revealedCount++;
            }
            else if (state == CellState.Flagged)
            {
                _flagsPlaced++;
            }
        }

        Status = snapshot.Status;
    }
}
