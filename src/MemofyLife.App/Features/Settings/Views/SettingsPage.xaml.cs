using MemofyLife.Features.Settings.ViewModels;

namespace MemofyLife.Features.Settings.Views;

public sealed partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
