using MemofyLife.Data;
using MemofyLife.Vehicle.Domain;
using MemofyLife.Vehicle.Persistence;
using SQLite;

namespace MemofyLife.Vehicle.Services;

/// <summary>Érvénytelen űrlapadat; az üzenet a felhasználónak szól.</summary>
public sealed class VehicleValidationException : Exception
{
    public VehicleValidationException(string message)
        : base(message)
    {
    }
}

/// <summary>Jármű áttekintő számai: aktuális km, e havi költség, átlagfogyasztás.</summary>
public sealed record VehicleSummary(int CurrentKm, int MonthCost, double? AverageConsumption);

internal static class VehicleQueries
{
    // Az aktuális km-állás nincs tárolva: a kezdő érték és a rögzített maximum közül a nagyobb.
    public const string CurrentOdometer =
        """
        SELECT MAX(v.initial_odometer_km,
                   COALESCE((SELECT MAX(odometer_km) FROM vehicle_entry WHERE vehicle_id = v.id), 0))
        FROM vehicle v
        WHERE v.id = ?
        """;
}

/// <summary>Szerviz rögzítésekor a kategóriához tartozó aktív emlékeztetők teljesítése.</summary>
internal static class ReminderCompletion
{
    public static void Apply(
        SQLiteConnection tx,
        VehicleEntryEntity entry,
        int categoryId,
        string now)
    {
        List<VehicleReminderEntity> reminders = tx.Query<VehicleReminderEntity>(
            "SELECT * FROM vehicle_reminder WHERE vehicle_id = ? AND category_id = ? AND is_active = 1",
            entry.VehicleId,
            categoryId);

        DateOnly date = DbValues.ParseDate(entry.EntryDate);

        foreach (VehicleReminderEntity reminder in reminders)
        {
            // Egy régebbi szerviz utólagos rögzítése nem írja felül a frissebb teljesítést.
            if (reminder.LastDoneKm is { } lastKm && entry.OdometerKm < lastKm)
            {
                continue;
            }

            if (DbValues.ParseNullableDate(reminder.LastDoneDate) is { } lastDate && date < lastDate)
            {
                continue;
            }

            reminder.LastDoneKm = entry.OdometerKm;
            reminder.LastDoneDate = entry.EntryDate;
            reminder.LastEntryId = entry.Id;
            reminder.UpdatedAt = now;

            if (MaintenanceSchedule.IsRecurring(reminder.IntervalKm, reminder.IntervalMonths))
            {
                (int? dueKm, DateOnly? dueDate) = MaintenanceSchedule.ComputeDue(
                    reminder.IntervalKm, reminder.IntervalMonths, entry.OdometerKm, date);
                reminder.DueKm = dueKm;
                reminder.DueDate = DbValues.ToDbDate(dueDate);
            }
            else
            {
                // Az egyszeri emlékeztető teljesítés után lezárul.
                reminder.IsActive = false;
            }

            tx.Update(reminder);
        }
    }
}
