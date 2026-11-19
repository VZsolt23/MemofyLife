using MemofyLife.Tests.Data;
using MemofyLife.Vehicle.Domain;
using MemofyLife.Vehicle.Models;
using MemofyLife.Vehicle.Services;

namespace MemofyLife.Tests.Vehicle;

public sealed class VehicleServiceTests : IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 11, 19);
    private const int OilChange = 1;
    private const int Brakes = 2;

    private readonly TestDatabase _db = new();
    private readonly VehicleService _vehicles;
    private readonly ReminderService _reminders;
    private int _vehicleId;

    public VehicleServiceTests()
    {
        var time = new FixedTimeProvider(new DateTimeOffset(2026, 11, 19, 9, 0, 0, TimeSpan.Zero));
        _vehicles = new VehicleService(_db.Database, time);
        _reminders = new ReminderService(_db.Database, time);
    }

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        _vehicleId = await _vehicles.SaveVehicleAsync(new VehicleProfile
        {
            Name = "Saját autó",
            LicensePlate = " abc-123 ",
            InitialOdometerKm = 100_000,
        });
    }

    public Task DisposeAsync()
    {
        return _db.DisposeAsync();
    }

    [Fact]
    public async Task Vehicle_IsSavedAndCleaned()
    {
        VehicleProfile? vehicle = await _vehicles.GetActiveVehicleAsync();

        Assert.NotNull(vehicle);
        Assert.Equal(_vehicleId, vehicle.Id);
        Assert.Equal("ABC-123", vehicle.LicensePlate);
        Assert.Null(vehicle.Make);
    }

    [Fact]
    public async Task InvalidInput_Throws()
    {
        await Assert.ThrowsAsync<VehicleValidationException>(() =>
            _vehicles.SaveEntryAsync(Entry(EntryType.Fuel, 100_100)));
    }

    [Fact]
    public async Task CurrentOdometer_FallsBackToInitial()
    {
        Assert.Equal(100_000, await _vehicles.GetCurrentOdometerAsync(_vehicleId));

        await _vehicles.SaveEntryAsync(Entry(EntryType.Odometer, 101_500));

        Assert.Equal(101_500, await _vehicles.GetCurrentOdometerAsync(_vehicleId));
    }

    [Fact]
    public async Task Log_ContainsDetailsAndConsumption()
    {
        await _vehicles.SaveEntryAsync(Entry(EntryType.Fuel, 100_000, Today.AddDays(-10)) with
        {
            Liters = 40,
            TotalCost = 24_000,
        });
        await _vehicles.SaveEntryAsync(Entry(EntryType.Fuel, 100_500) with
        {
            Liters = 35,
            TotalCost = 21_000,
        });
        await _vehicles.SaveEntryAsync(Entry(EntryType.Service, 100_600) with
        {
            CategoryId = Brakes,
            TotalCost = 50_000,
            Description = "Első fékbetét",
        });

        IReadOnlyList<LogEntry> log = await _vehicles.GetLogAsync(_vehicleId);

        Assert.Equal(3, log.Count);
        LogEntry service = log[0];
        Assert.Equal(EntryType.Service, service.Type);
        Assert.Equal("Fékek", service.CategoryName);

        LogEntry lastFuel = log.Single(e => e.OdometerKm == 100_500);
        Assert.Equal(7.0, lastFuel.ConsumptionPer100Km!.Value, 6);
        Assert.Equal(600.0, lastFuel.PricePerLiter!.Value, 6);

        IReadOnlyList<LogEntry> fuelOnly = await _vehicles.GetLogAsync(_vehicleId, EntryType.Fuel);
        Assert.All(fuelOnly, e => Assert.Equal(EntryType.Fuel, e.Type));

        VehicleSummary summary = await _vehicles.GetSummaryAsync(_vehicleId);
        Assert.Equal(100_600, summary.CurrentKm);
        Assert.Equal(95_000, summary.MonthCost);
        Assert.Equal(7.0, summary.AverageConsumption!.Value, 6);
    }

    [Fact]
    public async Task EditingEntry_CanChangeType()
    {
        int id = await _vehicles.SaveEntryAsync(Entry(EntryType.Fuel, 100_500) with { Liters = 30 });

        EntryInput? loaded = await _vehicles.GetEntryAsync(id);
        Assert.NotNull(loaded);
        Assert.Equal(30, loaded.Liters);

        await _vehicles.SaveEntryAsync(loaded with { Type = EntryType.Service, CategoryId = Brakes });

        LogEntry entry = Assert.Single(await _vehicles.GetLogAsync(_vehicleId));
        Assert.Equal(EntryType.Service, entry.Type);
        Assert.Null(entry.Liters);
    }

    [Fact]
    public async Task OdometerEntry_DropsCost()
    {
        await _vehicles.SaveEntryAsync(Entry(EntryType.Odometer, 100_500) with { TotalCost = 999 });

        Assert.Null(Assert.Single(await _vehicles.GetLogAsync(_vehicleId)).TotalCost);
    }

    [Fact]
    public async Task DeleteEntry_RemovesDetails()
    {
        int id = await _vehicles.SaveEntryAsync(Entry(EntryType.Fuel, 100_500) with { Liters = 30 });

        await _vehicles.DeleteEntryAsync(id);

        Assert.Empty(await _vehicles.GetLogAsync(_vehicleId));
        Assert.Null(await _vehicles.GetEntryAsync(id));
    }

    [Fact]
    public async Task CheckOdometer_WarnsAgainstNeighbours()
    {
        await _vehicles.SaveEntryAsync(Entry(EntryType.Odometer, 101_000, Today.AddDays(-5)));
        int later = await _vehicles.SaveEntryAsync(Entry(EntryType.Odometer, 102_000));

        Assert.Equal(
            OdometerWarning.BelowEarlierEntry,
            await _vehicles.CheckOdometerAsync(_vehicleId, Today.AddDays(-1), 100_900));
        Assert.Equal(
            OdometerWarning.AboveLaterEntry,
            await _vehicles.CheckOdometerAsync(_vehicleId, Today.AddDays(-1), 102_500));
        Assert.Equal(
            OdometerWarning.None,
            await _vehicles.CheckOdometerAsync(_vehicleId, Today, 101_500, later));
        Assert.Equal(
            OdometerWarning.BelowInitial,
            await _vehicles.CheckOdometerAsync(_vehicleId, Today, 99_000));
    }

    [Fact]
    public async Task ServiceEntry_CompletesRecurringReminder()
    {
        int reminderId = await _reminders.SaveReminderAsync(new ReminderInput
        {
            VehicleId = _vehicleId,
            Title = "Olajcsere",
            CategoryId = OilChange,
            IntervalKm = 15_000,
            IntervalMonths = 12,
            LastDoneKm = 90_000,
            LastDoneDate = new DateOnly(2026, 1, 10),
        });

        ReminderInfo before = Assert.Single(await _reminders.GetRemindersAsync(_vehicleId));
        Assert.Equal(105_000, before.DueKm);
        Assert.Equal(ReminderState.Ok, before.Status.State);

        await _vehicles.SaveEntryAsync(Entry(EntryType.Service, 104_500) with
        {
            CategoryId = OilChange,
        });

        ReminderInfo after = Assert.Single(await _reminders.GetRemindersAsync(_vehicleId));
        Assert.Equal(reminderId, after.Id);
        Assert.Equal(104_500, after.LastDoneKm);
        Assert.Equal(119_500, after.DueKm);
        Assert.Equal(Today.AddMonths(12), after.DueDate);
    }

    [Fact]
    public async Task OlderServiceEntry_DoesNotOverrideNewerCompletion()
    {
        await _reminders.SaveReminderAsync(new ReminderInput
        {
            VehicleId = _vehicleId,
            Title = "Olajcsere",
            CategoryId = OilChange,
            IntervalKm = 15_000,
            LastDoneKm = 100_000,
        });

        await _vehicles.SaveEntryAsync(Entry(EntryType.Service, 95_000, Today.AddYears(-1)) with
        {
            CategoryId = OilChange,
        });

        Assert.Equal(115_000, Assert.Single(await _reminders.GetRemindersAsync(_vehicleId)).DueKm);
    }

    [Fact]
    public async Task ServiceEntry_ClosesOneOffReminder()
    {
        await _reminders.SaveReminderAsync(new ReminderInput
        {
            VehicleId = _vehicleId,
            Title = "Műszaki vizsga",
            CategoryId = 4,
            DueDate = Today.AddDays(20),
        });

        Assert.Equal(
            ReminderState.DueSoon,
            (await _reminders.GetMostUrgentAsync(_vehicleId))!.Status.State);

        await _vehicles.SaveEntryAsync(Entry(EntryType.Service, 100_100) with { CategoryId = 4 });

        Assert.Empty(await _reminders.GetRemindersAsync(_vehicleId));
        ReminderInfo closed = Assert.Single(
            await _reminders.GetRemindersAsync(_vehicleId, includeInactive: true));
        Assert.False(closed.IsActive);
    }

    [Fact]
    public async Task Reminders_AreSortedByUrgency()
    {
        await _reminders.SaveReminderAsync(new ReminderInput
        {
            VehicleId = _vehicleId,
            Title = "Gumicsere",
            DueDate = Today.AddDays(100),
        });
        await _reminders.SaveReminderAsync(new ReminderInput
        {
            VehicleId = _vehicleId,
            Title = "Olajcsere",
            DueKm = 99_000,
        });

        IReadOnlyList<ReminderInfo> list = await _reminders.GetRemindersAsync(_vehicleId);

        Assert.Equal(["Olajcsere", "Gumicsere"], list.Select(r => r.Title));
        Assert.Equal(ReminderState.Overdue, list[0].Status.State);
    }

    [Fact]
    public async Task DeletingVehicle_CascadesToLogAndReminders()
    {
        await _vehicles.SaveEntryAsync(Entry(EntryType.Fuel, 100_500) with { Liters = 30 });
        await _reminders.SaveReminderAsync(new ReminderInput
        {
            VehicleId = _vehicleId,
            Title = "Olajcsere",
            DueKm = 110_000,
        });

        SQLite.SQLiteAsyncConnection c = await _db.Database.GetConnectionAsync();
        await c.ExecuteAsync("DELETE FROM vehicle WHERE id = ?", _vehicleId);

        Assert.Equal(0, await c.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM vehicle_entry"));
        Assert.Equal(0, await c.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM fuel_detail"));
        Assert.Equal(0, await c.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM vehicle_reminder"));
    }

    private EntryInput Entry(EntryType type, int km, DateOnly? date = null)
    {
        return new EntryInput
        {
            VehicleId = _vehicleId,
            Type = type,
            Date = date ?? Today,
            OdometerKm = km,
        };
    }
}
