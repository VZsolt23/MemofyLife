using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemofyLife.Common.Formatting;
using MemofyLife.Common.Interfaces;
using MemofyLife.Common.ViewModels;
using MemofyLife.Features.Vehicle.ViewModels;
using MemofyLife.Navigation;
using MemofyLife.Vehicle.Models;
using MemofyLife.Vehicle.Services;

namespace MemofyLife.Features.Home.ViewModels;

public sealed partial class HomeViewModel : BaseViewModel
{
    private readonly VehicleService _vehicles;
    private readonly ReminderService _reminders;
    private readonly INavigationService _nav;

    [ObservableProperty]
    private bool hasVehicle;

    [ObservableProperty]
    private string vehicleName = string.Empty;

    [ObservableProperty]
    private string currentKmText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasReminder))]
    private ReminderItemViewModel? nextReminder;

    public HomeViewModel(
        VehicleService vehicles,
        ReminderService reminders,
        INavigationService nav)
    {
        _vehicles = vehicles;
        _reminders = reminders;
        _nav = nav;
        Title = "Kezdőlap";
    }

    public bool HasReminder => NextReminder is not null;

    public async Task LoadAsync()
    {
        VehicleProfile? vehicle = await _vehicles.GetActiveVehicleAsync();
        HasVehicle = vehicle is not null;

        if (vehicle is null)
        {
            NextReminder = null;
            return;
        }

        VehicleName = vehicle.Name;
        CurrentKmText = HungarianFormat.Km(await _vehicles.GetCurrentOdometerAsync(vehicle.Id));

        ReminderInfo? urgent = await _reminders.GetMostUrgentAsync(vehicle.Id);
        NextReminder = urgent is null ? null : ReminderItemViewModel.From(urgent);
    }

    [RelayCommand]
    private Task OpenVehicleAsync()
    {
        return _nav.GoToTabAsync(Routes.VehicleTab);
    }
}
