using MemofyLife.Features.Vehicle.ViewModels;

namespace MemofyLife.Features.Vehicle.Views;

public sealed partial class VehicleLogPage : ContentPage
{
    private readonly VehicleLogViewModel _vm;

    public VehicleLogPage(VehicleLogViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
