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

public sealed partial class EntryEditViewModel : BaseViewModel, IQueryAttributable
{
    private readonly VehicleService _vehicles;
    private readonly INavigationService _nav;
    private readonly IDialogService _dialogs;
    private int _entryId;
    private int _vehicleId;
    private bool _loaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOdometer))]
    [NotifyPropertyChangedFor(nameof(IsFuel))]
    [NotifyPropertyChangedFor(nameof(IsService))]
    [NotifyPropertyChangedFor(nameof(HasCost))]
    private EntryType selectedType = EntryType.Fuel;

    [ObservableProperty]
    private DateTime? date = DateTime.Today;

    [ObservableProperty]
    private string? odometerText;

    [ObservableProperty]
    private string? lastKmHint;

    [ObservableProperty]
    private string? costText;

    [ObservableProperty]
    private string? litersText;

    [ObservableProperty]
    private bool isFullTank = true;

    [ObservableProperty]
    private IReadOnlyList<ServiceCategory> categories = [];

    [ObservableProperty]
    private ServiceCategory? selectedCategory;

    [ObservableProperty]
    private string? description;

    [ObservableProperty]
    private string? workshop;

    [ObservableProperty]
    private string? note;

    [ObservableProperty]
    private bool isEdit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorText;

    public EntryEditViewModel(
        VehicleService vehicles,
        INavigationService nav,
        IDialogService dialogs)
    {
        _vehicles = vehicles;
        _nav = nav;
        _dialogs = dialogs;
        Title = "Új bejegyzés";
    }

    public bool IsOdometer => SelectedType == EntryType.Odometer;

    public bool IsFuel => SelectedType == EntryType.Fuel;

    public bool IsService => SelectedType == EntryType.Service;

    public bool HasCost => !IsOdometer;

    public bool HasError => !string.IsNullOrEmpty(ErrorText);

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(Routes.Params.EntryId, out object? id) && id is int entryId)
        {
            _entryId = entryId;
        }

        if (query.TryGetValue(Routes.Params.EntryType, out object? type) && type is EntryType entryType)
        {
            SelectedType = entryType;
        }
    }

    public async Task InitializeAsync()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        Categories = await _vehicles.GetCategoriesAsync();

        if (_entryId != 0)
        {
            await LoadEntryAsync();
            return;
        }

        VehicleProfile? vehicle = await _vehicles.GetActiveVehicleAsync();

        if (vehicle is null)
        {
            await _dialogs.AlertAsync("Nincs jármű", "Előbb add meg az autó adatait.");
            await _nav.BackAsync();
            return;
        }

        _vehicleId = vehicle.Id;
        Date = _vehicles.Today.ToDateTime(TimeOnly.MinValue);

        int currentKm = await _vehicles.GetCurrentOdometerAsync(vehicle.Id);
        LastKmHint = $"Eddigi legnagyobb: {HungarianFormat.Km(currentKm)}";
    }

    [RelayCommand]
    private void SelectType(string type)
    {
        SelectedType = Enum.Parse<EntryType>(type);
        ErrorText = null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        EntryInput? input = BuildInput();

        if (input is null)
        {
            return;
        }

        ErrorText = InputValidator.Validate(input);

        if (ErrorText is not null)
        {
            return;
        }

        OdometerWarning warning = await _vehicles.CheckOdometerAsync(
            input.VehicleId, input.Date, input.OdometerKm, input.Id);

        if (warning != OdometerWarning.None
            && !await _dialogs.ConfirmAsync("Biztosan jó a km-állás?", Describe(warning), "Mentés", "Javítom"))
        {
            return;
        }

        try
        {
            await _vehicles.SaveEntryAsync(input);
            await _nav.BackAsync();
        }
        catch (VehicleValidationException ex)
        {
            ErrorText = ex.Message;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_entryId == 0
            || !await _dialogs.ConfirmAsync("Bejegyzés törlése", "Biztosan törlöd ezt a bejegyzést?", "Törlés"))
        {
            return;
        }

        await _vehicles.DeleteEntryAsync(_entryId);
        await _nav.BackAsync();
    }

    private async Task LoadEntryAsync()
    {
        EntryInput? entry = await _vehicles.GetEntryAsync(_entryId);

        if (entry is null)
        {
            await _nav.BackAsync();
            return;
        }

        Title = "Bejegyzés szerkesztése";
        IsEdit = true;
        _vehicleId = entry.VehicleId;
        SelectedType = entry.Type;
        Date = entry.Date.ToDateTime(TimeOnly.MinValue);
        OdometerText = HungarianFormat.ToInputText(entry.OdometerKm);
        CostText = HungarianFormat.ToInputText(entry.TotalCost);
        LitersText = HungarianFormat.ToInputText(entry.Liters);
        IsFullTank = entry.IsFullTank;
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == entry.CategoryId);
        Description = entry.Description;
        Workshop = entry.Workshop;
        Note = entry.Note;

        int currentKm = await _vehicles.GetCurrentOdometerAsync(entry.VehicleId);
        LastKmHint = $"Eddigi legnagyobb: {HungarianFormat.Km(currentKm)}";
    }

    private EntryInput? BuildInput()
    {
        if (!HungarianFormat.TryParseInt(OdometerText, out int km))
        {
            ErrorText = "Add meg a km-állást.";
            return null;
        }

        if (!HungarianFormat.TryParseOptionalInt(HasCost ? CostText : null, out int? cost))
        {
            ErrorText = "A költség csak egész szám lehet (Ft).";
            return null;
        }

        double? liters = null;

        if (IsFuel)
        {
            if (!HungarianFormat.TryParseDouble(LitersText, out double parsed))
            {
                ErrorText = "Add meg a tankolt mennyiséget literben.";
                return null;
            }

            liters = parsed;
        }

        return new EntryInput
        {
            Id = _entryId,
            VehicleId = _vehicleId,
            Type = SelectedType,
            Date = DateOnly.FromDateTime(Date ?? DateTime.Today),
            OdometerKm = km,
            TotalCost = cost,
            Note = Note,
            Liters = liters,
            IsFullTank = IsFullTank,
            CategoryId = IsService ? SelectedCategory?.Id : null,
            Description = IsService ? Description : null,
            Workshop = IsService ? Workshop : null,
        };
    }

    private static string Describe(OdometerWarning warning)
    {
        return warning switch
        {
            OdometerWarning.BelowInitial => "Kisebb, mint a jármű kezdő km-állása.",
            OdometerWarning.BelowEarlierEntry => "Kisebb, mint egy korábbi bejegyzés km-állása.",
            OdometerWarning.AboveLaterEntry => "Nagyobb, mint egy későbbi bejegyzés km-állása.",
            _ => string.Empty,
        };
    }
}
