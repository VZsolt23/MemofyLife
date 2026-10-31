using MemofyLife.Features.Home.ViewModels;

namespace MemofyLife.Features.Home.Views;

public sealed partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
