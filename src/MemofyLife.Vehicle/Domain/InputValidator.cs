using MemofyLife.Vehicle.Models;

namespace MemofyLife.Vehicle.Domain;

/// <summary>Űrlapadatok ellenőrzése. Hibánál a felhasználónak szóló üzenetet adja, egyébként null-t.</summary>
public static class InputValidator
{
    public const int MinModelYear = 1900;

    public static string? Validate(VehicleProfile vehicle, int currentYear)
    {
        if (string.IsNullOrWhiteSpace(vehicle.Name))
        {
            return "Add meg a jármű nevét.";
        }

        if (vehicle.InitialOdometerKm < 0)
        {
            return "A kezdő km-állás nem lehet negatív.";
        }

        if (vehicle.ModelYear is { } year && (year < MinModelYear || year > currentYear + 1))
        {
            return $"Az évjárat {MinModelYear} és {currentYear + 1} között lehet.";
        }

        return null;
    }

    public static string? Validate(EntryInput entry)
    {
        if (entry.OdometerKm < 0)
        {
            return "A km-állás nem lehet negatív.";
        }

        if (entry.TotalCost is < 0)
        {
            return "A költség nem lehet negatív.";
        }

        return entry.Type switch
        {
            EntryType.Fuel when entry.Liters is not > 0 => "Add meg a tankolt mennyiséget.",
            EntryType.Service when entry.CategoryId is null => "Válassz szervizkategóriát.",
            EntryType.Odometer or EntryType.Fuel or EntryType.Service => null,
            _ => "Ismeretlen bejegyzéstípus.",
        };
    }

    public static string? Validate(ReminderInput reminder)
    {
        if (string.IsNullOrWhiteSpace(reminder.Title))
        {
            return "Add meg az emlékeztető nevét.";
        }

        if (reminder.IntervalKm is <= 0 || reminder.IntervalMonths is <= 0)
        {
            return "Az intervallum csak pozitív szám lehet.";
        }

        if (reminder.NotifyBeforeKm is < 0 || reminder.NotifyBeforeDays is < 0)
        {
            return "Az előzetes jelzés nem lehet negatív.";
        }

        if (reminder.IsRecurring)
        {
            if (reminder.IntervalKm is not null && reminder.LastDoneKm is null)
            {
                return "Add meg, hány km-nél volt utoljára.";
            }

            if (reminder.IntervalMonths is not null && reminder.LastDoneDate is null)
            {
                return "Add meg, mikor volt utoljára.";
            }

            return null;
        }

        return reminder.DueKm is null && reminder.DueDate is null
            ? "Adj meg esedékes km-állást vagy dátumot."
            : null;
    }
}
