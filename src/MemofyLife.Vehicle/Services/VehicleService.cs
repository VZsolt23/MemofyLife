using MemofyLife.Data;
using MemofyLife.Vehicle.Domain;
using MemofyLife.Vehicle.Models;
using MemofyLife.Vehicle.Persistence;
using SQLite;

namespace MemofyLife.Vehicle.Services;

/// <summary>Jármű törzsadatai és a napló (km-rögzítés, tankolás, szerviz).</summary>
public sealed class VehicleService
{
    private const string LogSelect =
        """
        SELECT e.id, e.vehicle_id, e.entry_type, e.entry_date, e.odometer_km, e.total_cost, e.note,
               f.liters, f.is_full_tank,
               s.category_id, c.name AS category_name, s.description, s.workshop
        FROM vehicle_entry e
        LEFT JOIN fuel_detail f ON f.entry_id = e.id
        LEFT JOIN service_detail s ON s.entry_id = e.id
        LEFT JOIN service_category c ON c.id = s.category_id
        """;

    private readonly AppDatabase _db;
    private readonly TimeProvider _time;

    public VehicleService(AppDatabase db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public DateOnly Today => DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

    // ---------------------------------------------------------------
    // Jármű
    // ---------------------------------------------------------------

    /// <summary>Az első nem archivált jármű; a v1 felülete egy járművet kezel.</summary>
    public async Task<VehicleProfile?> GetActiveVehicleAsync()
    {
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();

        List<VehicleEntity> rows = await c.QueryAsync<VehicleEntity>(
            "SELECT * FROM vehicle WHERE is_archived = 0 ORDER BY id LIMIT 1");

        return rows.Count == 0 ? null : ToProfile(rows[0]);
    }

    public async Task<VehicleProfile?> GetVehicleAsync(int vehicleId)
    {
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();
        VehicleEntity? entity = await c.FindAsync<VehicleEntity>(vehicleId);
        return entity is null ? null : ToProfile(entity);
    }

    public async Task<int> SaveVehicleAsync(VehicleProfile profile)
    {
        ThrowIfInvalid(InputValidator.Validate(profile, Today.Year));

        SQLiteAsyncConnection c = await _db.GetConnectionAsync();
        string now = Now();

        VehicleEntity entity = profile.Id == 0
            ? new VehicleEntity { CreatedAt = now }
            : await c.FindAsync<VehicleEntity>(profile.Id)
              ?? throw new InvalidOperationException("A jármű nem található.");

        entity.Name = profile.Name.Trim();
        entity.Make = Clean(profile.Make);
        entity.Model = Clean(profile.Model);
        entity.ModelYear = profile.ModelYear;
        entity.LicensePlate = Clean(profile.LicensePlate)?.ToUpperInvariant();
        entity.FuelType = profile.FuelType;
        entity.InitialOdometerKm = profile.InitialOdometerKm;
        entity.UpdatedAt = now;

        if (profile.Id == 0)
        {
            await c.InsertAsync(entity);
        }
        else
        {
            await c.UpdateAsync(entity);
        }

        return entity.Id;
    }

    // ---------------------------------------------------------------
    // Összesítők
    // ---------------------------------------------------------------

    /// <summary>A kezdő km és a legnagyobb rögzített km közül a nagyobb.</summary>
    public async Task<int> GetCurrentOdometerAsync(int vehicleId)
    {
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();
        return await c.ExecuteScalarAsync<int>(VehicleQueries.CurrentOdometer, vehicleId);
    }

    public async Task<VehicleSummary> GetSummaryAsync(int vehicleId)
    {
        DateOnly today = Today;
        int currentKm = await GetCurrentOdometerAsync(vehicleId);
        int monthCost = await GetMonthlyCostAsync(vehicleId, today.Year, today.Month);
        double? consumption = ConsumptionCalculator.Average(await GetFuelFillsAsync(vehicleId));

        return new VehicleSummary(currentKm, monthCost, consumption);
    }

    public async Task<int> GetMonthlyCostAsync(int vehicleId, int year, int month)
    {
        var first = new DateOnly(year, month, 1);
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();

        return await c.ExecuteScalarAsync<int>(
            """
            SELECT COALESCE(SUM(total_cost), 0) FROM vehicle_entry
            WHERE vehicle_id = ? AND entry_date >= ? AND entry_date < ?
            """,
            vehicleId,
            DbValues.ToDbDate(first),
            DbValues.ToDbDate(first.AddMonths(1)));
    }

    // ---------------------------------------------------------------
    // Napló
    // ---------------------------------------------------------------

    public async Task<IReadOnlyList<ServiceCategory>> GetCategoriesAsync()
    {
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();

        List<ServiceCategoryEntity> rows = await c.QueryAsync<ServiceCategoryEntity>(
            "SELECT * FROM service_category ORDER BY sort_order, name");

        return rows.ConvertAll(r => new ServiceCategory(r.Id, r.Name));
    }

    /// <summary>A napló időrendben, legújabb elöl; opcionálisan típusra szűrve és korlátozva.</summary>
    public async Task<IReadOnlyList<LogEntry>> GetLogAsync(
        int vehicleId,
        EntryType? type = null,
        int? limit = null)
    {
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();

        var sql = $"{LogSelect} WHERE e.vehicle_id = ?";
        var args = new List<object> { vehicleId };

        if (type is not null)
        {
            sql += " AND e.entry_type = ?";
            args.Add((int)type.Value);
        }

        sql += " ORDER BY e.entry_date DESC, e.id DESC";

        if (limit is not null)
        {
            sql += " LIMIT ?";
            args.Add(limit.Value);
        }

        List<LogRow> rows = await c.QueryAsync<LogRow>(sql, [.. args]);

        IReadOnlyDictionary<int, double> consumption = rows.Any(r => r.EntryType == EntryType.Fuel)
            ? ConsumptionCalculator.PerFill(await GetFuelFillsAsync(vehicleId))
            : new Dictionary<int, double>();

        return rows.ConvertAll(r => ToLogEntry(r, consumption));
    }

    public async Task<EntryInput?> GetEntryAsync(int entryId)
    {
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();
        List<LogRow> rows = await c.QueryAsync<LogRow>($"{LogSelect} WHERE e.id = ?", entryId);

        if (rows.Count == 0)
        {
            return null;
        }

        LogRow r = rows[0];

        return new EntryInput
        {
            Id = r.Id,
            VehicleId = r.VehicleId,
            Type = r.EntryType,
            Date = DbValues.ParseDate(r.EntryDate),
            OdometerKm = r.OdometerKm,
            TotalCost = r.TotalCost,
            Note = r.Note,
            Liters = r.Liters,
            IsFullTank = r.IsFullTank ?? true,
            CategoryId = r.CategoryId,
            Description = r.Description,
            Workshop = r.Workshop,
        };
    }

    /// <summary>
    /// Fejléc és részletező mentése egy tranzakcióban. Szerviznél a kategóriához tartozó
    /// aktív emlékeztetőket teljesítettnek jelöli.
    /// </summary>
    public async Task<int> SaveEntryAsync(EntryInput input)
    {
        ThrowIfInvalid(InputValidator.Validate(input));

        SQLiteAsyncConnection c = await _db.GetConnectionAsync();
        string now = Now();
        var savedId = 0;

        await c.RunInTransactionAsync(tx =>
        {
            bool isNew = input.Id == 0;

            VehicleEntryEntity entry = isNew
                ? new VehicleEntryEntity { CreatedAt = now }
                : tx.Find<VehicleEntryEntity>(input.Id)
                  ?? throw new InvalidOperationException("A bejegyzés nem található.");

            entry.VehicleId = input.VehicleId;
            entry.EntryType = input.Type;
            entry.EntryDate = DbValues.ToDbDate(input.Date);
            entry.OdometerKm = input.OdometerKm;
            entry.TotalCost = input.Type == EntryType.Odometer ? null : input.TotalCost;
            entry.Note = Clean(input.Note);
            entry.UpdatedAt = now;

            if (isNew)
            {
                tx.Insert(entry);
            }
            else
            {
                tx.Update(entry);
            }

            // A típus szerkesztéskor változhat, ezért mindkét részletezőt újraírjuk.
            tx.Execute("DELETE FROM fuel_detail WHERE entry_id = ?", entry.Id);
            tx.Execute("DELETE FROM service_detail WHERE entry_id = ?", entry.Id);

            if (input.Type == EntryType.Fuel)
            {
                tx.Insert(new FuelDetailEntity
                {
                    EntryId = entry.Id,
                    Liters = input.Liters!.Value,
                    IsFullTank = input.IsFullTank,
                });
            }
            else if (input.Type == EntryType.Service)
            {
                tx.Insert(new ServiceDetailEntity
                {
                    EntryId = entry.Id,
                    CategoryId = input.CategoryId!.Value,
                    Description = Clean(input.Description),
                    Workshop = Clean(input.Workshop),
                });

                ReminderCompletion.Apply(tx, entry, input.CategoryId.Value, now);
            }

            savedId = entry.Id;
        });

        return savedId;
    }

    public async Task DeleteEntryAsync(int entryId)
    {
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();
        await c.ExecuteAsync("DELETE FROM vehicle_entry WHERE id = ?", entryId);
    }

    /// <summary>Figyelmeztetés, ha a km-állás nem illik a szomszédos bejegyzések közé.</summary>
    public async Task<OdometerWarning> CheckOdometerAsync(
        int vehicleId,
        DateOnly date,
        int km,
        int excludeEntryId = 0)
    {
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();
        string day = DbValues.ToDbDate(date);

        int initial = await c.ExecuteScalarAsync<int>(
            "SELECT initial_odometer_km FROM vehicle WHERE id = ?",
            vehicleId);

        int? maxBefore = await c.ExecuteScalarAsync<int?>(
            "SELECT MAX(odometer_km) FROM vehicle_entry WHERE vehicle_id = ? AND entry_date <= ? AND id <> ?",
            vehicleId,
            day,
            excludeEntryId);

        int? minAfter = await c.ExecuteScalarAsync<int?>(
            "SELECT MIN(odometer_km) FROM vehicle_entry WHERE vehicle_id = ? AND entry_date > ? AND id <> ?",
            vehicleId,
            day,
            excludeEntryId);

        return OdometerValidator.Check(km, initial, maxBefore, minAfter);
    }

    // ---------------------------------------------------------------
    // Segédek
    // ---------------------------------------------------------------

    internal static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    internal static void ThrowIfInvalid(string? error)
    {
        if (error is not null)
        {
            throw new VehicleValidationException(error);
        }
    }

    private string Now()
    {
        return DbValues.ToDbTimestamp(_time.GetUtcNow());
    }

    private async Task<List<FuelFill>> GetFuelFillsAsync(int vehicleId)
    {
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();

        List<LogRow> rows = await c.QueryAsync<LogRow>(
            """
            SELECT e.id, e.odometer_km, f.liters, f.is_full_tank
            FROM vehicle_entry e
            JOIN fuel_detail f ON f.entry_id = e.id
            WHERE e.vehicle_id = ?
            """,
            vehicleId);

        return rows.ConvertAll(r => new FuelFill(r.Id, r.OdometerKm, r.Liters ?? 0, r.IsFullTank ?? false));
    }

    private static VehicleProfile ToProfile(VehicleEntity e)
    {
        return new VehicleProfile
        {
            Id = e.Id,
            Name = e.Name,
            Make = e.Make,
            Model = e.Model,
            ModelYear = e.ModelYear,
            LicensePlate = e.LicensePlate,
            FuelType = e.FuelType,
            InitialOdometerKm = e.InitialOdometerKm,
        };
    }

    private static LogEntry ToLogEntry(LogRow r, IReadOnlyDictionary<int, double> consumption)
    {
        double? pricePerLiter = r.TotalCost is not null && r.Liters is > 0
            ? r.TotalCost.Value / r.Liters.Value
            : null;

        return new LogEntry
        {
            Id = r.Id,
            Type = r.EntryType,
            Date = DbValues.ParseDate(r.EntryDate),
            OdometerKm = r.OdometerKm,
            TotalCost = r.TotalCost,
            Note = r.Note,
            Liters = r.Liters,
            IsFullTank = r.IsFullTank,
            PricePerLiter = pricePerLiter,
            ConsumptionPer100Km = consumption.TryGetValue(r.Id, out double value) ? value : null,
            CategoryId = r.CategoryId,
            CategoryName = r.CategoryName,
            Description = r.Description,
            Workshop = r.Workshop,
        };
    }
}
