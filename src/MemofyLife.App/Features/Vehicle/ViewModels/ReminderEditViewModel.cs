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

public sealed partial class ReminderEditViewModel : BaseViewModel, IQueryAttributable
{
    // A Pickerben a „nincs kategória” választás; az adatbázisba null kerül.
    private static readonly ServiceCategory NoCategory = new(0, "Nincs");

    private readonly ReminderService _reminders;
    private readonly VehicleService _vehicles;
    private readonly INavigationService _nav;
    private readonly IDialogService _dialogs;
    private int _reminderId;
    private int _vehicleId;
    private bool _loaded;

    [ObservableProperty]
    private string reminderTitle = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<ServiceCategory> categories = [];

    [ObservableProperty]
    private ServiceCategory? selectedCategory;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOneOff))]
    private bool isRecurring = true;

    [ObservableProperty]
    private string? intervalKmText;

    [ObservableProperty]
    private string? intervalMonthsText;

    [ObservableProperty]
    private string? lastDoneKmText;

    [ObservableProperty]
    private DateTime? lastDoneDate = DateTime.Today;

    [ObservableProperty]
    private string? dueKmText;

    [ObservableProperty]
    private bool hasDueDate;

    [ObservableProperty]
    private DateTime? dueDate = DateTime.Today;

    [ObservableProperty]
    private string? notifyBeforeKmText;

    [ObservableProperty]
    private string? notifyBeforeDaysText;

    [ObservableProperty]
    private bool isActive = true;

    [ObservableProperty]
    private bool isEdit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorText;

    public ReminderEditViewModel(
        ReminderService reminders,
        VehicleService vehicles,
        INavigationService nav,
        IDialogService dialogs)
    {
        _reminders = reminders;
        _vehicles = vehicles;
        _nav = nav;
        _dialogs = dialogs;
        Title = "Új emlékeztető";
    }

    public bool IsOneOff => !IsRecurring;

    public bool HasError => !string.IsNullOrEmpty(ErrorText);

    public string NotifyKmPlaceholder => $"{MaintenanceSchedule.DefaultNotifyBeforeKm}";

    public string NotifyDaysPlaceholder => $"{MaintenanceSchedule.DefaultNotifyBeforeDays}";

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(Routes.Params.ReminderId, out object? value) && value is int id)
        {
            _reminderId = id;
        }
    }

    /// <summary>Kategória választásakor üres névnél a kategória nevét javasoljuk.</summary>
    partial void OnSelectedCategoryChanged(ServiceCategory? value)
    {
        if (value is not null && value != NoCategory && string.IsNullOrWhiteSpace(ReminderTitle))
        {
            ReminderTitle = value.Name;
        }
    }

    public async Task InitializeAsync()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        Categories = [NoCategory, .. await _vehicles.GetCategoriesAsync()];

        if (_reminderId != 0)
        {
            await LoadReminderAsync();
            return;
        }

        VehicleProfile? vehicle = await _vehicles.GetActiveVehicleAsync();

        if (vehicle is null)
        {
            await _nav.BackAsync();
            return;
        }

        _vehicleId = vehicle.Id;
        SelectedCategory = NoCategory;
        LastDoneKmText = HungarianFormat.ToInputText(
            await _vehicles.GetCurrentOdometerAsync(vehicle.Id));
        LastDoneDate = _vehicles.Today.ToDateTime(TimeOnly.MinValue);
        DueDate = LastDoneDate;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ReminderInput? input = BuildInput();

        if (input is null)
        {
            return;
        }

        ErrorText = InputValidator.Validate(input);

        if (ErrorText is not null)
        {
            return;
        }

        try
        {
            await _reminders.SaveReminderAsync(input);
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
        if (_reminderId == 0
            || !await _dialogs.ConfirmAsync("Emlékeztető törlése", "Biztosan törlöd?", "Törlés"))
        {
            return;
        }

        await _reminders.DeleteReminderAsync(_reminderId);
        await _nav.BackAsync();
    }

    private async Task LoadReminderAsync()
    {
        ReminderInput? r = await _reminders.GetReminderAsync(_reminderId);

        if (r is null)
        {
            await _nav.BackAsync();
            return;
        }

        Title = "Emlékeztető szerkesztése";
        IsEdit = true;
        _vehicleId = r.VehicleId;
        ReminderTitle = r.Title;
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == r.CategoryId) ?? NoCategory;
        IsRecurring = r.IsRecurring;
        IntervalKmText = HungarianFormat.ToInputText(r.IntervalKm);
        IntervalMonthsText = HungarianFormat.ToInputText(r.IntervalMonths);
        LastDoneKmText = HungarianFormat.ToInputText(r.LastDoneKm);
        LastDoneDate = r.LastDoneDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today;
        DueKmText = HungarianFormat.ToInputText(r.DueKm);
        HasDueDate = r.DueDate is not null;
        DueDate = r.DueDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today;
        NotifyBeforeKmText = HungarianFormat.ToInputText(r.NotifyBeforeKm);
        NotifyBeforeDaysText = HungarianFormat.ToInputText(r.NotifyBeforeDays);
        IsActive = r.IsActive;
    }

    private ReminderInput? BuildInput()
    {
        if (!TryParse(IntervalKmText, "Az intervallum (km)", out int? intervalKm)
            || !TryParse(IntervalMonthsText, "Az intervallum (hónap)", out int? intervalMonths)
            || !TryParse(LastDoneKmText, "Az utolsó km-állás", out int? lastDoneKm)
            || !TryParse(DueKmText, "Az esedékes km-állás", out int? dueKm)
            || !TryParse(NotifyBeforeKmText, "Az előzetes jelzés (km)", out int? notifyKm)
            || !TryParse(NotifyBeforeDaysText, "Az előzetes jelzés (nap)", out int? notifyDays))
        {
            return null;
        }

        if (IsRecurring && intervalKm is null && intervalMonths is null)
        {
            ErrorText = "Adj meg km- vagy hónap-intervallumot.";
            return null;
        }

        return new ReminderInput
        {
            Id = _reminderId,
            VehicleId = _vehicleId,
            Title = ReminderTitle,
            CategoryId = SelectedCategory is { Id: > 0 } c ? c.Id : null,
            IntervalKm = IsRecurring ? intervalKm : null,
            IntervalMonths = IsRecurring ? intervalMonths : null,
            LastDoneKm = IsRecurring ? lastDoneKm : null,
            LastDoneDate = IsRecurring && LastDoneDate is { } done ? DateOnly.FromDateTime(done) : null,
            DueKm = IsRecurring ? null : dueKm,
            DueDate = !IsRecurring && HasDueDate && DueDate is { } due ? DateOnly.FromDateTime(due) : null,
            NotifyBeforeKm = notifyKm,
            NotifyBeforeDays = notifyDays,
            IsActive = IsActive,
        };
    }

    private bool TryParse(string? text, string field, out int? value)
    {
        if (HungarianFormat.TryParseOptionalInt(text, out value))
        {
            return true;
        }

        ErrorText = $"{field} csak egész szám lehet.";
        return false;
    }
}
