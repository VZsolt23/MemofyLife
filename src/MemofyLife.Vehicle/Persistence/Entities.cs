using MemofyLife.Vehicle.Domain;
using SQLite;

namespace MemofyLife.Vehicle.Persistence;

// A sémát az M001_Vehicle migráció hozza létre; ezek az osztályok csak leképezik.
// Dátum: yyyy-MM-dd szöveg, időbélyeg: ISO 8601 UTC szöveg (lásd DbValues).

[Table("vehicle")]
internal sealed class VehicleEntity
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("make")]
    public string? Make { get; set; }

    [Column("model")]
    public string? Model { get; set; }

    [Column("model_year")]
    public int? ModelYear { get; set; }

    [Column("license_plate")]
    public string? LicensePlate { get; set; }

    [Column("fuel_type")]
    public FuelType? FuelType { get; set; }

    [Column("initial_odometer_km")]
    public int InitialOdometerKm { get; set; }

    [Column("is_archived")]
    public bool IsArchived { get; set; }

    [Column("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [Column("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;
}

[Table("vehicle_entry")]
internal sealed class VehicleEntryEntity
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("vehicle_id")]
    public int VehicleId { get; set; }

    [Column("entry_type")]
    public EntryType EntryType { get; set; }

    [Column("entry_date")]
    public string EntryDate { get; set; } = string.Empty;

    [Column("odometer_km")]
    public int OdometerKm { get; set; }

    [Column("total_cost")]
    public int? TotalCost { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [Column("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [Column("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;
}

[Table("fuel_detail")]
internal sealed class FuelDetailEntity
{
    [PrimaryKey, Column("entry_id")]
    public int EntryId { get; set; }

    [Column("liters")]
    public double Liters { get; set; }

    [Column("is_full_tank")]
    public bool IsFullTank { get; set; }
}

[Table("service_detail")]
internal sealed class ServiceDetailEntity
{
    [PrimaryKey, Column("entry_id")]
    public int EntryId { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("workshop")]
    public string? Workshop { get; set; }
}

[Table("service_category")]
internal sealed class ServiceCategoryEntity
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("is_system")]
    public bool IsSystem { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }
}

[Table("vehicle_reminder")]
internal sealed class VehicleReminderEntity
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("vehicle_id")]
    public int VehicleId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("interval_km")]
    public int? IntervalKm { get; set; }

    [Column("interval_months")]
    public int? IntervalMonths { get; set; }

    [Column("last_done_km")]
    public int? LastDoneKm { get; set; }

    [Column("last_done_date")]
    public string? LastDoneDate { get; set; }

    [Column("last_entry_id")]
    public int? LastEntryId { get; set; }

    [Column("due_km")]
    public int? DueKm { get; set; }

    [Column("due_date")]
    public string? DueDate { get; set; }

    [Column("notify_before_km")]
    public int? NotifyBeforeKm { get; set; }

    [Column("notify_before_days")]
    public int? NotifyBeforeDays { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [Column("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;
}

/// <summary>A napló lekérdezésének lapos sora (bejegyzés + részletezők + kategórianév).</summary>
internal sealed class LogRow
{
    [Column("id")]
    public int Id { get; set; }

    [Column("vehicle_id")]
    public int VehicleId { get; set; }

    [Column("entry_type")]
    public EntryType EntryType { get; set; }

    [Column("entry_date")]
    public string EntryDate { get; set; } = string.Empty;

    [Column("odometer_km")]
    public int OdometerKm { get; set; }

    [Column("total_cost")]
    public int? TotalCost { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [Column("liters")]
    public double? Liters { get; set; }

    [Column("is_full_tank")]
    public bool? IsFullTank { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("category_name")]
    public string? CategoryName { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("workshop")]
    public string? Workshop { get; set; }
}
