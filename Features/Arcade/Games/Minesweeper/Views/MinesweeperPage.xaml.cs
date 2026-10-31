using MemofyLife.Features.Arcade.Games.Minesweeper.ViewModels;

namespace MemofyLife.Features.Arcade.Games.Minesweeper.Views;

public partial class MinesweeperPage : ContentPage
{
    public MinesweeperPage(MinesweeperViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
