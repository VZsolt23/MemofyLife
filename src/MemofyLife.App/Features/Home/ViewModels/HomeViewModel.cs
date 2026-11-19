using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemofyLife.Common.Formatting;
using MemofyLife.Common.Interfaces;
using MemofyLife.Common.Theming;
using MemofyLife.Common.ViewModels;
using MemofyLife.Features.Arcade.Catalog;
using MemofyLife.Features.Arcade.Shared;
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
    private readonly TimeProvider _time;

    [ObservableProperty]
    private bool hasVehicle;

    [ObservableProperty]
    private string greeting = string.Empty;

    [ObservableProperty]
    private string todayText = string.Empty;

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
        IEnumerable<IGame> games,
        INavigationService nav,
        TimeProvider time)
    {
        _vehicles = vehicles;
        _reminders = reminders;
        _nav = nav;
        _time = time;
        Title = "Kezdőlap";
        FeaturedGame = games.FirstOrDefault();
    }

    public bool HasReminder => NextReminder is not null;

    public IGame? FeaturedGame { get; }

    public bool HasFeaturedGame => FeaturedGame is not null;

    public ImageSource? FeaturedGameIcon => FeaturedGame?.Icon;

    public Color ArcadeColor => SemanticColors.Arcade;

    public async Task LoadAsync()
    {
        DateTime now = _time.GetLocalNow().DateTime;
        Greeting = now.Hour switch
        {
            < 9 => "Jó reggelt!",
            < 18 => "Szia!",
            _ => "Jó estét!",
        };
        string day = now.ToString("dddd, MMMM d.", HungarianFormat.Culture);
        TodayText = char.ToUpper(day[0], HungarianFormat.Culture) + day[1..];

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

    [RelayCommand]
    private Task OpenSettingsAsync()
    {
        return _nav.GoToAsync(Routes.SettingsPage);
    }

    [RelayCommand]
    private Task OpenGameAsync()
    {
        return FeaturedGame is null ? Task.CompletedTask : _nav.OpenGameAsync(FeaturedGame);
    }
}
