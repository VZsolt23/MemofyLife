using MemofyLife.Vehicle.Domain;

namespace MemofyLife.Vehicle.Models;

/// <summary>A jármű törzsadatai. <see cref="Id"/> = 0 új járművet jelöl.</summary>
public sealed record VehicleProfile
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Make { get; init; }
    public string? Model { get; init; }
    public int? ModelYear { get; init; }
    public string? LicensePlate { get; init; }
    public FuelType? FuelType { get; init; }
    public int InitialOdometerKm { get; init; }
}

/// <summary>Napló-bejegyzés mentéshez és szerkesztéshez. <see cref="Id"/> = 0 új bejegyzés.</summary>
public sealed record EntryInput
{
    public int Id { get; init; }
    public required int VehicleId { get; init; }
    public required EntryType Type { get; init; }
    public required DateOnly Date { get; init; }
    public required int OdometerKm { get; init; }
    public int? TotalCost { get; init; }
    public string? Note { get; init; }

    // Tankolás
    public double? Liters { get; init; }
    public bool IsFullTank { get; init; } = true;

    // Szerviz
    public int? CategoryId { get; init; }
    public string? Description { get; init; }
    public string? Workshop { get; init; }
}

/// <summary>A napló egy sora, a származtatott értékekkel (Ft/liter, fogyasztás).</summary>
public sealed record LogEntry
{
    public required int Id { get; init; }
    public required EntryType Type { get; init; }
    public required DateOnly Date { get; init; }
    public required int OdometerKm { get; init; }
    public int? TotalCost { get; init; }
    public string? Note { get; init; }
    public double? Liters { get; init; }
    public bool? IsFullTank { get; init; }
    public double? PricePerLiter { get; init; }
    public double? ConsumptionPer100Km { get; init; }
    public int? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? Description { get; init; }
    public string? Workshop { get; init; }
}

public sealed record ServiceCategory(int Id, string Name);

/// <summary>
/// Karbantartási emlékeztető. Ismétlődő, ha van km- vagy hónap-intervalluma; ekkor az
/// esedékesség az utolsó elvégzésből számolódik. Egyszeri esetben <see cref="DueKm"/> /
/// <see cref="DueDate"/> közvetlenül megadott.
/// </summary>
public sealed record ReminderInput
{
    public int Id { get; init; }
    public required int VehicleId { get; init; }
    public required string Title { get; init; }
    public int? CategoryId { get; init; }
    public int? IntervalKm { get; init; }
    public int? IntervalMonths { get; init; }
    public int? LastDoneKm { get; init; }
    public DateOnly? LastDoneDate { get; init; }
    public int? DueKm { get; init; }
    public DateOnly? DueDate { get; init; }
    public int? NotifyBeforeKm { get; init; }
    public int? NotifyBeforeDays { get; init; }
    public bool IsActive { get; init; } = true;

    public bool IsRecurring => MaintenanceSchedule.IsRecurring(IntervalKm, IntervalMonths);
}

public sealed record ReminderInfo
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public int? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public int? IntervalKm { get; init; }
    public int? IntervalMonths { get; init; }
    public int? LastDoneKm { get; init; }
    public DateOnly? LastDoneDate { get; init; }
    public int? DueKm { get; init; }
    public DateOnly? DueDate { get; init; }
    public required bool IsActive { get; init; }
    public required MaintenanceStatus Status { get; init; }
}
