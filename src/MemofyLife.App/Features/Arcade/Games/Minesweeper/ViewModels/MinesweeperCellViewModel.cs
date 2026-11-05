using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemofyLife.Minesweeper.Engine;
using MemofyLife.Minesweeper.Engine.Enums;

namespace MemofyLife.Features.Arcade.Games.Minesweeper.ViewModels;

public sealed partial class MinesweeperCellViewModel : ObservableObject
{
    public MinesweeperCellViewModel(
        int row,
        int column,
        Action<MinesweeperCellViewModel> onTap,
        Action<MinesweeperCellViewModel> onLongPress)
    {
        Row = row;
        Column = column;
        TapCommand = new RelayCommand(() => onTap(this));
        LongPressCommand = new RelayCommand(() => onLongPress(this));
    }

    public ICommand TapCommand { get; }

    public ICommand LongPressCommand { get; }

    public int Row { get; }

    public int Column { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Text))]
    [NotifyPropertyChangedFor(nameof(IsRevealed))]
    private CellState state;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Text))]
    private bool isMine;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Text))]
    private int adjacentMines;

    public bool IsRevealed => State == CellState.Revealed;

    public string Text {
        get {
            if (State == CellState.Flagged)
            {
                return "🚩";
            }

            if (IsMine)
            {
                return "💣";
            }

            return State == CellState.Revealed && AdjacentMines > 0
                ? AdjacentMines.ToString()
                : string.Empty;
        }
    }

    public void Update(CellView view)
    {
        State = view.State;
        IsMine = view.IsMine;
        AdjacentMines = view.AdjacentMines;
    }
}
