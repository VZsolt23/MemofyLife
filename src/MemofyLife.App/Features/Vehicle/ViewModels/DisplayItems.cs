using MemofyLife.Common.Formatting;
using MemofyLife.Vehicle.Domain;
using MemofyLife.Vehicle.Models;

namespace MemofyLife.Features.Vehicle.ViewModels;

/// <summary>A napló egy sora megjelenítésre előkészítve.</summary>
public sealed record LogItemViewModel(
    int Id,
    string Icon,
    string Title,
    string Subtitle,
    string? Detail,
    string? CostText)
{
    public bool HasDetail => !string.IsNullOrEmpty(Detail);

    public static LogItemViewModel From(LogEntry e)
    {
        string subtitle = $"{HungarianFormat.Date(e.Date)} · {HungarianFormat.Km(e.OdometerKm)}";
        string? cost = e.TotalCost is { } c ? HungarianFormat.Forint(c) : null;

        return e.Type switch
        {
            EntryType.Fuel => new LogItemViewModel(
                e.Id,
                "⛽",
                FuelTitle(e),
                subtitle,
                Join(FuelDetail(e), e.Note),
                cost),
            EntryType.Service => new LogItemViewModel(
                e.Id,
                "🔧",
                Join(e.CategoryName ?? "Szerviz", e.Description, separator: " · ")!,
                subtitle,
                Join(e.Workshop, e.Note),
                cost),
            _ => new LogItemViewModel(e.Id, "📍", "Km-állás", subtitle, e.Note, cost),
        };
    }

    private static string FuelTitle(LogEntry e)
    {
        string title = e.Liters is { } l ? $"Tankolás · {HungarianFormat.Liters(l)}" : "Tankolás";
        return e.IsFullTank == false ? $"{title} (részleges)" : title;
    }

    private static string? FuelDetail(LogEntry e)
    {
        return Join(
            e.PricePerLiter is { } p ? HungarianFormat.PricePerLiter(p) : null,
            e.ConsumptionPer100Km is { } c ? HungarianFormat.Consumption(c) : null,
            separator: " · ");
    }

    private static string? Join(string? a, string? b, string separator = "\n")
    {
        string[] parts = new[] { a, b }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray()!;
        return parts.Length == 0 ? null : string.Join(separator, parts);
    }
}

/// <summary>Karbantartási emlékeztető megjelenítésre előkészítve.</summary>
public sealed record ReminderItemViewModel(
    int Id,
    string Title,
    string StatusText,
    string? DueText,
    Color StatusColor,
    bool IsActive)
{
    public static ReminderItemViewModel From(ReminderInfo r)
    {
        return new ReminderItemViewModel(
            r.Id,
            r.Title,
            r.IsActive ? ReminderText.Status(r.Status) : "Lezárva",
            ReminderText.Due(r.DueKm, r.DueDate),
            r.IsActive ? ReminderText.StateColor(r.Status.State) : Colors.Gray,
            r.IsActive);
    }
}

public static class ReminderText
{
    public static string Status(MaintenanceStatus status)
    {
        var parts = new List<string>();

        if (status.KmLeft is { } km)
        {
            parts.Add(km >= 0
                ? $"{HungarianFormat.Km(km)} múlva"
                : $"{HungarianFormat.Km(-km)}-rel túllépve");
        }

        if (status.DaysLeft is { } days)
        {
            parts.Add(days switch
            {
                > 0 => $"{days} nap múlva",
                0 => "ma esedékes",
                _ => $"{-days} napja lejárt",
            });
        }

        return parts.Count == 0 ? "Nincs esedékesség" : string.Join(" · ", parts);
    }

    public static string? Due(int? dueKm, DateOnly? dueDate)
    {
        var parts = new List<string>();

        if (dueKm is { } km)
        {
            parts.Add(HungarianFormat.Km(km));
        }

        if (dueDate is { } date)
        {
            parts.Add(HungarianFormat.Date(date));
        }

        return parts.Count == 0 ? null : $"Esedékes: {string.Join(" · ", parts)}";
    }

    public static Color StateColor(ReminderState state)
    {
        return state switch
        {
            ReminderState.Overdue => Color.FromArgb("#D93025"),
            ReminderState.DueSoon => Color.FromArgb("#E37400"),
            ReminderState.Ok => Color.FromArgb("#188038"),
            _ => Colors.Gray,
        };
    }
}

public sealed record FuelTypeOption(FuelType Value, string Label)
{
    public static IReadOnlyList<FuelTypeOption> All { get; } =
    [
        new(FuelType.Petrol, "Benzin"),
        new(FuelType.Diesel, "Dízel"),
        new(FuelType.Hybrid, "Hibrid"),
        new(FuelType.Electric, "Elektromos"),
        new(FuelType.Lpg, "LPG"),
    ];

    public override string ToString()
    {
        return Label;
    }
}
