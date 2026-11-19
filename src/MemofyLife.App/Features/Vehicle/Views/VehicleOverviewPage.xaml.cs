using MemofyLife.Features.Vehicle.ViewModels;

namespace MemofyLife.Features.Vehicle.Views;

public sealed partial class VehicleOverviewPage : ContentPage
{
    private readonly VehicleOverviewViewModel _vm;

    public VehicleOverviewPage(VehicleOverviewViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    // Minden megjelenéskor frissít: a szerkesztő oldalakról visszatérve is friss az adat.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
