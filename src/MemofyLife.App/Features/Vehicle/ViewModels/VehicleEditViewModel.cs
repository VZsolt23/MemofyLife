using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemofyLife.Common.Formatting;
using MemofyLife.Common.Interfaces;
using MemofyLife.Common.ViewModels;
using MemofyLife.Navigation;
using MemofyLife.Vehicle.Domain;
using MemofyLife.Vehicle.Models;
using MemofyLife.Vehicle.Services;

namespace MemofyLife.Features.Vehicle.ViewModels;

public sealed partial class VehicleEditViewModel : BaseViewModel, IQueryAttributable
{
    private readonly VehicleService _vehicles;
    private readonly INavigationService _nav;
    private int _vehicleId;
    private bool _loaded;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string? make;

    [ObservableProperty]
    private string? model;

    [ObservableProperty]
    private string? modelYearText;

    [ObservableProperty]
    private string? licensePlate;

    [ObservableProperty]
    private FuelTypeOption? selectedFuelType;

    [ObservableProperty]
    private string? initialKmText;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorText;

    public VehicleEditViewModel(VehicleService vehicles, INavigationService nav)
    {
        _vehicles = vehicles;
        _nav = nav;
        Title = "Új jármű";
    }

    public IReadOnlyList<FuelTypeOption> FuelTypes => FuelTypeOption.All;

    public bool HasError => !string.IsNullOrEmpty(ErrorText);

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(Routes.Params.VehicleId, out object? value) && value is int id)
        {
            _vehicleId = id;
        }
    }

    public async Task InitializeAsync()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;

        if (_vehicleId == 0)
        {
            Name = "Saját autó";
            InitialKmText = "0";
            return;
        }

        VehicleProfile? vehicle = await _vehicles.GetVehicleAsync(_vehicleId);

        if (vehicle is null)
        {
            await _nav.BackAsync();
            return;
        }

        Title = "Jármű szerkesztése";
        Name = vehicle.Name;
        Make = vehicle.Make;
        Model = vehicle.Model;
        ModelYearText = HungarianFormat.ToInputText(vehicle.ModelYear);
        LicensePlate = vehicle.LicensePlate;
        SelectedFuelType = FuelTypes.FirstOrDefault(f => f.Value == vehicle.FuelType);
        InitialKmText = HungarianFormat.ToInputText(vehicle.InitialOdometerKm);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!HungarianFormat.TryParseOptionalInt(ModelYearText, out int? modelYear))
        {
            ErrorText = "Az évjárat csak szám lehet.";
            return;
        }

        if (!HungarianFormat.TryParseOptionalInt(InitialKmText, out int? initialKm))
        {
            ErrorText = "A kezdő km-állás csak szám lehet.";
            return;
        }

        var profile = new VehicleProfile
        {
            Id = _vehicleId,
            Name = Name,
            Make = Make,
            Model = Model,
            ModelYear = modelYear,
            LicensePlate = LicensePlate,
            FuelType = SelectedFuelType?.Value,
            InitialOdometerKm = initialKm ?? 0,
        };

        ErrorText = InputValidator.Validate(profile, _vehicles.Today.Year);

        if (ErrorText is not null)
        {
            return;
        }

        try
        {
            await _vehicles.SaveVehicleAsync(profile);
            await _nav.BackAsync();
        }
        catch (VehicleValidationException ex)
        {
            ErrorText = ex.Message;
        }
    }
}
