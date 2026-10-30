using MemofyLife.Features.Home.ViewModels;

namespace MemofyLife.Features.Home.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
