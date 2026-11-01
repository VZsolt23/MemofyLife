using MemofyLife.Features.Arcade.Games.Minesweeper.ViewModels;

namespace MemofyLife.Features.Arcade.Games.Minesweeper.Views;

public sealed partial class MinesweeperPage : ContentPage
{
    public MinesweeperPage(MinesweeperViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
