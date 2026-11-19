using MemofyLife.Features.Vehicle.ViewModels;

namespace MemofyLife.Features.Vehicle.Views;

public sealed partial class ReminderEditPage : ContentPage
{
    private readonly ReminderEditViewModel _vm;

    public ReminderEditPage(ReminderEditViewModel vm)
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
