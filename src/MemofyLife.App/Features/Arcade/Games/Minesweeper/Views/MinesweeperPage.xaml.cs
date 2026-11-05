using MemofyLife.Features.Arcade.Games.Minesweeper.ViewModels;
using System.ComponentModel;

namespace MemofyLife.Features.Arcade.Games.Minesweeper.Views;

public sealed partial class MinesweeperPage : ContentPage
{
    private const double CellSpacing = 2;

    private readonly MinesweeperViewModel _vm;

    public MinesweeperPage(MinesweeperViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
        _vm.PropertyChanged += OnViewModelPropertyChanged;
        UpdateBoardLayout();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Resume();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Pause();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MinesweeperViewModel.Columns))
        {
            UpdateBoardLayout();
        }
    }

    // A GridItemsLayout nem tud a Span-ra kötni (nem Element, nem örökli a BindingContextet), ezért kódból állítjuk.
    private void UpdateBoardLayout()
    {
        Board.ItemsLayout = new GridItemsLayout(Math.Max(_vm.Columns, 1), ItemsLayoutOrientation.Vertical)
        {
            HorizontalItemSpacing = CellSpacing,
            VerticalItemSpacing = CellSpacing,
        };
    }

    // A cellák négyzetesek: a magasságot a (a Span által meghatározott) szélességhez igazítjuk.
    private void OnCellSizeChanged(object? sender, EventArgs e)
    {
        if (sender is VisualElement cell && cell.Width > 0 && Math.Abs(cell.HeightRequest - cell.Width) > 0.5)
        {
            cell.HeightRequest = cell.Width;
        }
    }
}
