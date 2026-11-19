using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemofyLife.Common.Formatting;
using MemofyLife.Common.Interfaces;
using MemofyLife.Common.ViewModels;
using MemofyLife.Vehicle.Domain;
using MemofyLife.Vehicle.Models;
using MemofyLife.Vehicle.Services;

namespace MemofyLife.Features.Vehicle.ViewModels;

public sealed partial class VehicleOverviewViewModel : BaseViewModel
{
    private const int RecentEntryCount = 5;

    private readonly VehicleService _vehicles;
    private readonly ReminderService _reminders;
    private readonly INavigationService _nav;
    private int _vehicleId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoVehicle))]
    private bool hasVehicle;

    [ObservableProperty]
    private bool isLoaded;

    [ObservableProperty]
    private string vehicleName = string.Empty;

    [ObservableProperty]
    private string? vehicleDetails;

    [ObservableProperty]
    private string currentKmText = string.Empty;

    [ObservableProperty]
    private string monthCostText = string.Empty;

    [ObservableProperty]
    private string consumptionText = "–";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasReminders))]
    private IReadOnlyList<ReminderItemViewModel> reminders = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEntries))]
    private IReadOnlyList<LogItemViewModel> recentEntries = [];

    public VehicleOverviewViewModel(
        VehicleService vehicles,
        ReminderService reminders,
        INavigationService nav)
    {
        _vehicles = vehicles;
        _reminders = reminders;
        _nav = nav;
        Title = "Jármű";
    }

    public bool HasNoVehicle => IsLoaded && !HasVehicle;

    public bool HasReminders => Reminders.Count > 0;

    public bool HasEntries => RecentEntries.Count > 0;

    partial void OnIsLoadedChanged(bool value)
    {
        OnPropertyChanged(nameof(HasNoVehicle));
    }

    public async Task LoadAsync()
    {
        VehicleProfile? vehicle = await _vehicles.GetActiveVehicleAsync();

        if (vehicle is null)
        {
            _vehicleId = 0;
            HasVehicle = false;
            Reminders = [];
            RecentEntries = [];
            IsLoaded = true;
            return;
        }

        _vehicleId = vehicle.Id;
        VehicleName = vehicle.Name;
        VehicleDetails = Describe(vehicle);

        VehicleSummary summary = await _vehicles.GetSummaryAsync(vehicle.Id);
        CurrentKmText = HungarianFormat.Km(summary.CurrentKm);
        MonthCostText = HungarianFormat.Forint(summary.MonthCost);
        ConsumptionText = summary.AverageConsumption is { } c
            ? HungarianFormat.Consumption(c)
            : "–";

        IReadOnlyList<ReminderInfo> reminders = await _reminders.GetRemindersAsync(vehicle.Id);
        Reminders = reminders.Select(ReminderItemViewModel.From).ToList();

        IReadOnlyList<LogEntry> entries = await _vehicles.GetLogAsync(
            vehicle.Id,
            limit: RecentEntryCount);
        RecentEntries = entries.Select(LogItemViewModel.From).ToList();

        HasVehicle = true;
        IsLoaded = true;
    }

    [RelayCommand]
    private Task AddVehicleAsync()
    {
        return _nav.OpenVehicleEditAsync();
    }

    [RelayCommand]
    private Task EditVehicleAsync()
    {
        return _nav.OpenVehicleEditAsync(_vehicleId);
    }

    [RelayCommand]
    private Task AddEntryAsync(string type)
    {
        return _nav.OpenNewEntryAsync(Enum.Parse<EntryType>(type));
    }

    [RelayCommand]
    private Task OpenEntryAsync(LogItemViewModel item)
    {
        return _nav.OpenEntryAsync(item.Id);
    }

    [RelayCommand]
    private Task OpenLogAsync()
    {
        return _nav.OpenVehicleLogAsync();
    }

    [RelayCommand]
    private Task AddReminderAsync()
    {
        return _nav.OpenReminderAsync();
    }

    [RelayCommand]
    private Task OpenReminderAsync(ReminderItemViewModel item)
    {
        return _nav.OpenReminderAsync(item.Id);
    }

    private static string? Describe(VehicleProfile v)
    {
        string?[] parts =
        [
            string.Join(' ', new[] { v.Make, v.Model }.Where(s => !string.IsNullOrEmpty(s))),
            v.ModelYear?.ToString(HungarianFormat.Culture),
            v.LicensePlate,
        ];

        string[] present = parts.Where(p => !string.IsNullOrEmpty(p)).ToArray()!;
        return present.Length == 0 ? null : string.Join(" · ", present);
    }
}
