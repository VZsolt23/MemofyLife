using MemofyLife.Features.Arcade.Catalog.ViewModels;

namespace MemofyLife.Features.Arcade.Catalog.Views;

public sealed partial class ArcadePage : ContentPage
{
    public ArcadePage(ArcadeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
