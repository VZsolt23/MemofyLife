using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemofyLife.Common.Interfaces;
using MemofyLife.Common.ViewModels;
using MemofyLife.Vehicle.Domain;
using MemofyLife.Vehicle.Models;
using MemofyLife.Vehicle.Services;

namespace MemofyLife.Features.Vehicle.ViewModels;

public sealed partial class VehicleLogViewModel : BaseViewModel
{
    private readonly VehicleService _vehicles;
    private readonly INavigationService _nav;
    private int _vehicleId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAll))]
    [NotifyPropertyChangedFor(nameof(IsOdometer))]
    [NotifyPropertyChangedFor(nameof(IsFuel))]
    [NotifyPropertyChangedFor(nameof(IsService))]
    private EntryType? filter;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private IReadOnlyList<LogItemViewModel> items = [];

    [ObservableProperty]
    private bool isRefreshing;

    public VehicleLogViewModel(VehicleService vehicles, INavigationService nav)
    {
        _vehicles = vehicles;
        _nav = nav;
        Title = "Napló";
    }

    public bool IsAll => Filter is null;

    public bool IsOdometer => Filter == EntryType.Odometer;

    public bool IsFuel => Filter == EntryType.Fuel;

    public bool IsService => Filter == EntryType.Service;

    public bool IsEmpty => Items.Count == 0;

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            VehicleProfile? vehicle = await _vehicles.GetActiveVehicleAsync();
            _vehicleId = vehicle?.Id ?? 0;

            IReadOnlyList<LogEntry> entries = vehicle is null
                ? []
                : await _vehicles.GetLogAsync(vehicle.Id, Filter);

            Items = entries.Select(LogItemViewModel.From).ToList();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private Task SetFilterAsync(string? type)
    {
        Filter = string.IsNullOrEmpty(type) ? null : Enum.Parse<EntryType>(type);
        return LoadAsync();
    }

    [RelayCommand]
    private Task OpenEntryAsync(LogItemViewModel item)
    {
        return _nav.OpenEntryAsync(item.Id);
    }

    [RelayCommand]
    private Task AddEntryAsync()
    {
        return _vehicleId == 0
            ? Task.CompletedTask
            : _nav.OpenNewEntryAsync(Filter ?? EntryType.Fuel);
    }
}
