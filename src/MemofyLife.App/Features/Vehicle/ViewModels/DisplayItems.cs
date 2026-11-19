using MemofyLife.Common.Formatting;
using MemofyLife.Common.Theming;
using MemofyLife.Vehicle.Domain;
using MemofyLife.Vehicle.Models;

namespace MemofyLife.Features.Vehicle.ViewModels;

/// <summary>A napló egy sora megjelenítésre előkészítve.</summary>
public sealed record LogItemViewModel(
    int Id,
    EntryType Type,
    DateOnly Date,
    int? Cost,
    string Title,
    string Subtitle,
    string? Detail)
{
    public bool HasDetail => !string.IsNullOrEmpty(Detail);

    public string? CostText => Cost is { } c ? HungarianFormat.Forint(c) : null;

    public ImageSource Icon => EntryVisuals.Icon(Type);

    public Color Color => SemanticColors.For(Type);

    public static LogItemViewModel From(LogEntry e)
    {
        string subtitle = $"{HungarianFormat.Date(e.Date)} · {HungarianFormat.Km(e.OdometerKm)}";

        return e.Type switch
        {
            EntryType.Fuel => new LogItemViewModel(
                e.Id,
                e.Type,
                e.Date,
                e.TotalCost,
                FuelTitle(e),
                subtitle,
                Join(FuelDetail(e), e.Note)),
            EntryType.Service => new LogItemViewModel(
                e.Id,
                e.Type,
                e.Date,
                e.TotalCost,
                Join(e.CategoryName ?? "Szerviz", e.Description, separator: " · ")!,
                subtitle,
                Join(e.Workshop, e.Note)),
            _ => new LogItemViewModel(e.Id, e.Type, e.Date, e.TotalCost, "Km-állás", subtitle, e.Note),
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
    string StatusLabel,
    string? DueText,
    Color StatusColor,
    bool IsActive)
{
    public ImageSource Icon => "wrench_solid_full.png";

    public static ReminderItemViewModel From(ReminderInfo r)
    {
        return new ReminderItemViewModel(
            r.Id,
            r.Title,
            r.IsActive ? ReminderText.Status(r.Status) : "Lezárva",
            r.IsActive ? ReminderText.Label(r.Status.State) : "Lezárva",
            ReminderText.Due(r.DueKm, r.DueDate),
            r.IsActive ? SemanticColors.For(r.Status.State) : SemanticColors.Neutral,
            r.IsActive);
    }
}

/// <summary>A napló egy hónapja: fejléc a havi összeggel, alatta a bejegyzések.</summary>
public sealed class LogMonthGroup : List<LogItemViewModel>
{
    public LogMonthGroup(DateOnly month, IEnumerable<LogItemViewModel> items)
        : base(items)
    {
        Title = month.ToString("yyyy. MMMM", HungarianFormat.Culture);
        int total = this.Sum(i => i.Cost ?? 0);
        TotalText = total > 0 ? HungarianFormat.Forint(total) : string.Empty;
    }

    public string Title { get; }

    public string TotalText { get; }

    public static IReadOnlyList<LogMonthGroup> Build(IEnumerable<LogItemViewModel> items)
    {
        return items
            .GroupBy(i => new DateOnly(i.Date.Year, i.Date.Month, 1))
            .Select(g => new LogMonthGroup(g.Key, g))
            .ToList();
    }
}

public static class EntryVisuals
{
    public static ImageSource Icon(EntryType type)
    {
        return type switch
        {
            EntryType.Fuel => "gas_pump_solid_full.png",
            EntryType.Service => "wrench_solid_full.png",
            _ => "gauge_high_solid_full.png",
        };
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

    public static string Label(ReminderState state)
    {
        return state switch
        {
            ReminderState.Overdue => "Lejárt",
            ReminderState.DueSoon => "Közeleg",
            ReminderState.Ok => "Rendben",
            _ => "Nincs adat",
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
