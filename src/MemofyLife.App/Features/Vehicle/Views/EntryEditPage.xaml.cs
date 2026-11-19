using MemofyLife.Features.Vehicle.ViewModels;

namespace MemofyLife.Features.Vehicle.Views;

public sealed partial class EntryEditPage : ContentPage
{
    private readonly EntryEditViewModel _vm;

    public EntryEditPage(EntryEditViewModel vm)
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
