using MemofyLife.Data;
using MemofyLife.Vehicle.Domain;
using MemofyLife.Vehicle.Models;
using MemofyLife.Vehicle.Persistence;
using SQLite;

namespace MemofyLife.Vehicle.Services;

/// <summary>Karbantartási emlékeztetők kezelése és kiértékelése.</summary>
public sealed class ReminderService
{
    private readonly AppDatabase _db;
    private readonly TimeProvider _time;

    public ReminderService(AppDatabase db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    /// <summary>Az emlékeztetők sürgősség szerint: lejárt, közeleg, rendben; az inaktívak a végén.</summary>
    public async Task<IReadOnlyList<ReminderInfo>> GetRemindersAsync(
        int vehicleId,
        bool includeInactive = false)
    {
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();

        int currentKm = await c.ExecuteScalarAsync<int>(VehicleQueries.CurrentOdometer, vehicleId);

        List<ReminderRow> rows = await c.QueryAsync<ReminderRow>(
            """
            SELECT r.*, c.name AS category_name
            FROM vehicle_reminder r
            LEFT JOIN service_category c ON c.id = r.category_id
            WHERE r.vehicle_id = ? AND (r.is_active = 1 OR ? = 1)
            """,
            vehicleId,
            includeInactive ? 1 : 0);

        DateOnly today = DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

        return rows
            .Select(r => ToInfo(r, currentKm, today))
            .OrderByDescending(r => r.IsActive)
            .ThenByDescending(r => r.Status.State)
            .ThenBy(r => r.Status.DaysLeft ?? int.MaxValue)
            .ThenBy(r => r.Status.KmLeft ?? int.MaxValue)
            .ThenBy(r => r.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>A legsürgősebb aktív emlékeztető, amelynek van esedékessége.</summary>
    public async Task<ReminderInfo?> GetMostUrgentAsync(int vehicleId)
    {
        IReadOnlyList<ReminderInfo> reminders = await GetRemindersAsync(vehicleId);
        return reminders.FirstOrDefault(r => r.Status.State != ReminderState.Unknown);
    }

    public async Task<ReminderInput?> GetReminderAsync(int reminderId)
    {
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();
        VehicleReminderEntity? r = await c.FindAsync<VehicleReminderEntity>(reminderId);

        if (r is null)
        {
            return null;
        }

        bool recurring = MaintenanceSchedule.IsRecurring(r.IntervalKm, r.IntervalMonths);

        return new ReminderInput
        {
            Id = r.Id,
            VehicleId = r.VehicleId,
            Title = r.Title,
            CategoryId = r.CategoryId,
            IntervalKm = r.IntervalKm,
            IntervalMonths = r.IntervalMonths,
            LastDoneKm = r.LastDoneKm,
            LastDoneDate = DbValues.ParseNullableDate(r.LastDoneDate),
            DueKm = recurring ? null : r.DueKm,
            DueDate = recurring ? null : DbValues.ParseNullableDate(r.DueDate),
            NotifyBeforeKm = r.NotifyBeforeKm,
            NotifyBeforeDays = r.NotifyBeforeDays,
            IsActive = r.IsActive,
        };
    }

    public async Task<int> SaveReminderAsync(ReminderInput input)
    {
        VehicleService.ThrowIfInvalid(InputValidator.Validate(input));

        SQLiteAsyncConnection c = await _db.GetConnectionAsync();
        string now = DbValues.ToDbTimestamp(_time.GetUtcNow());

        VehicleReminderEntity entity = input.Id == 0
            ? new VehicleReminderEntity { CreatedAt = now }
            : await c.FindAsync<VehicleReminderEntity>(input.Id)
              ?? throw new InvalidOperationException("Az emlékeztető nem található.");

        entity.VehicleId = input.VehicleId;
        entity.Title = input.Title.Trim();
        entity.CategoryId = input.CategoryId;
        entity.IntervalKm = input.IntervalKm;
        entity.IntervalMonths = input.IntervalMonths;
        entity.LastDoneKm = input.LastDoneKm;
        entity.LastDoneDate = DbValues.ToDbDate(input.LastDoneDate);
        entity.NotifyBeforeKm = input.NotifyBeforeKm;
        entity.NotifyBeforeDays = input.NotifyBeforeDays;
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = now;

        if (input.IsRecurring)
        {
            (int? dueKm, DateOnly? dueDate) = MaintenanceSchedule.ComputeDue(
                input.IntervalKm, input.IntervalMonths, input.LastDoneKm, input.LastDoneDate);
            entity.DueKm = dueKm;
            entity.DueDate = DbValues.ToDbDate(dueDate);
        }
        else
        {
            entity.DueKm = input.DueKm;
            entity.DueDate = DbValues.ToDbDate(input.DueDate);
        }

        if (input.Id == 0)
        {
            await c.InsertAsync(entity);
        }
        else
        {
            await c.UpdateAsync(entity);
        }

        return entity.Id;
    }

    public async Task DeleteReminderAsync(int reminderId)
    {
        SQLiteAsyncConnection c = await _db.GetConnectionAsync();
        await c.ExecuteAsync("DELETE FROM vehicle_reminder WHERE id = ?", reminderId);
    }

    private static ReminderInfo ToInfo(ReminderRow r, int currentKm, DateOnly today)
    {
        DateOnly? dueDate = DbValues.ParseNullableDate(r.DueDate);

        return new ReminderInfo
        {
            Id = r.Id,
            Title = r.Title,
            CategoryId = r.CategoryId,
            CategoryName = r.CategoryName,
            IntervalKm = r.IntervalKm,
            IntervalMonths = r.IntervalMonths,
            LastDoneKm = r.LastDoneKm,
            LastDoneDate = DbValues.ParseNullableDate(r.LastDoneDate),
            DueKm = r.DueKm,
            DueDate = dueDate,
            IsActive = r.IsActive,
            Status = MaintenanceSchedule.Evaluate(
                r.DueKm, dueDate, r.NotifyBeforeKm, r.NotifyBeforeDays, currentKm, today),
        };
    }

    private sealed class ReminderRow
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Column("category_name")]
        public string? CategoryName { get; set; }

        [Column("interval_km")]
        public int? IntervalKm { get; set; }

        [Column("interval_months")]
        public int? IntervalMonths { get; set; }

        [Column("last_done_km")]
        public int? LastDoneKm { get; set; }

        [Column("last_done_date")]
        public string? LastDoneDate { get; set; }

        [Column("due_km")]
        public int? DueKm { get; set; }

        [Column("due_date")]
        public string? DueDate { get; set; }

        [Column("notify_before_km")]
        public int? NotifyBeforeKm { get; set; }

        [Column("notify_before_days")]
        public int? NotifyBeforeDays { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
