using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemofyLife.Core.Interfaces;
using MemofyLife.Core.ViewModels;
using MemofyLife.Minesweeper.Engine;
using MemofyLife.Minesweeper.Engine.Enums;
using MemofyLife.Navigation;

namespace MemofyLife.Features.Arcade.Games.Minesweeper.ViewModels;

public sealed partial class MinesweeperViewModel : BaseViewModel, IQueryAttributable
{
    private readonly INavigationService _nav;
    private readonly IMinePlacer _minePlacer;
    private readonly TimeProvider _time;

    private MinesweeperBoard _board;
    private string _variantKey = MinesweeperDifficulty.Easy;
    private TimeSpan _accumulated;
    private long _segmentStart;
    private CancellationTokenSource? _clockCts;

    [ObservableProperty]
    private IReadOnlyList<MinesweeperCellViewModel> cells = [];

    [ObservableProperty]
    private int columns;

    [ObservableProperty]
    private int flagsLeft;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGameOver))]
    [NotifyPropertyChangedFor(nameof(ResultText))]
    private GameStatus status;

    [ObservableProperty]
    private string elapsedText = "00:00";

    [ObservableProperty]
    private bool isFlagMode;

    public MinesweeperViewModel(INavigationService nav, IMinePlacer minePlacer, TimeProvider time)
    {
        _nav = nav;
        _minePlacer = minePlacer;
        _time = time;
        Title = "Aknakereső";

        StartNewGame(_variantKey);
    }

    public bool IsGameOver => Status is GameStatus.Won or GameStatus.Lost;

    public string ResultText => Status switch
    {
        GameStatus.Won => "Nyertél!",
        GameStatus.Lost => "Játék vége",
        _ => string.Empty,
    };

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(Routes.Params.Variant, out var value) && value is string variant)
        {
            StartNewGame(variant);
        }
    }

    // Az oldal OnDisappearing-je és az app háttérbe kerülése hívja. Az adatréteg után ide kerül a mentés is.
    public void Pause()
    {
        StopClock();
    }

    public void Resume()
    {
        if (Status == GameStatus.Playing)
        {
            StartClock();
        }
    }

    // Egy koppintás: zászló módban zászlót tesz, egyébként felfed. A hosszú nyomás mindig zászlót tesz.
    [RelayCommand]
    private void Tap(MinesweeperCellViewModel cell)
    {
        if (IsFlagMode)
        {
            ToggleFlag(cell);
        }
        else
        {
            Reveal(cell);
        }
    }

    [RelayCommand]
    private void Reveal(MinesweeperCellViewModel cell)
    {
        _board.Reveal(cell.Row, cell.Column);
        AfterMove();
    }

    [RelayCommand]
    private void ToggleFlag(MinesweeperCellViewModel cell)
    {
        _board.ToggleFlag(cell.Row, cell.Column);
        AfterMove();
    }

    [RelayCommand]
    private void Restart()
    {
        StartNewGame(_variantKey);
    }

    [RelayCommand]
    private Task ExitAsync()
    {
        return _nav.BackAsync();
    }

    [MemberNotNull(nameof(_board))]
    private void StartNewGame(string variantKey)
    {
        StopClock();
        _accumulated = TimeSpan.Zero;
        ElapsedText = FormatElapsed(_accumulated);

        _variantKey = variantKey;
        BoardSettings settings = MinesweeperDifficulty.GetSettings(variantKey);
        _board = new MinesweeperBoard(settings, _minePlacer);

        var list = new List<MinesweeperCellViewModel>(settings.Rows * settings.Columns);

        for (var row = 0; row < settings.Rows; row++)
        {
            for (var col = 0; col < settings.Columns; col++)
            {
                list.Add(new MinesweeperCellViewModel(row, col, Tap, ToggleFlag));
            }
        }

        Cells = list;
        Columns = settings.Columns;
        Sync();
    }

    private void AfterMove()
    {
        Sync();

        if (Status == GameStatus.Playing)
        {
            StartClock();
        }
        else if (IsGameOver)
        {
            StopClock();
        }
    }

    private void Sync()
    {
        foreach (MinesweeperCellViewModel cell in Cells)
        {
            cell.Update(_board[cell.Row, cell.Column]);
        }

        FlagsLeft = _board.FlagsLeft;
        Status = _board.Status;
    }

    private void StartClock()
    {
        if (_clockCts is not null)
        {
            return;
        }

        _segmentStart = _time.GetTimestamp();
        _clockCts = new CancellationTokenSource();
        _ = RunClockAsync(_clockCts.Token);
    }

    private void StopClock()
    {
        if (_clockCts is null)
        {
            return;
        }

        _accumulated += _time.GetElapsedTime(_segmentStart);
        _clockCts.Cancel();
        _clockCts.Dispose();
        _clockCts = null;
        ElapsedText = FormatElapsed(_accumulated);
    }

    private async Task RunClockAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1), _time);

        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                ElapsedText = FormatElapsed(_accumulated + _time.GetElapsedTime(_segmentStart));
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        return $"{(int)elapsed.TotalMinutes:00}:{elapsed.Seconds:00}";
    }
}
