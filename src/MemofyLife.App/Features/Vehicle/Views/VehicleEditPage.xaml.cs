using MemofyLife.Features.Vehicle.ViewModels;

namespace MemofyLife.Features.Vehicle.Views;

public sealed partial class VehicleEditPage : ContentPage
{
    private readonly VehicleEditViewModel _vm;

    public VehicleEditPage(VehicleEditViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
    }
}
