using MemofyLife.Vehicle.Domain;

namespace MemofyLife.Common.Theming;

/// <summary>
/// Jelentést hordozó színek: a sémától és a világos/sötét módtól függetlenek. Középtónusok,
/// hogy világos és sötét felületen is olvashatók legyenek; a háttér ugyanez halványítva.
/// </summary>
public static class SemanticColors
{
    // Állapotok
    public static readonly Color Success = Color.FromArgb("#2E9E5B");
    public static readonly Color Warning = Color.FromArgb("#D98A00");
    public static readonly Color Danger = Color.FromArgb("#D93025");
    public static readonly Color Neutral = Color.FromArgb("#8A8A95");

    // Kategóriák
    public static readonly Color Odometer = Color.FromArgb("#378ADD");
    public static readonly Color Fuel = Color.FromArgb("#1D9E75");
    public static readonly Color Service = Color.FromArgb("#C47A12");
    public static readonly Color Events = Color.FromArgb("#D85A30");
    public static readonly Color Vehicle = Color.FromArgb("#378ADD");
    public static readonly Color Arcade = Color.FromArgb("#7F77DD");

    /// <summary>Halvány háttér egy kiemelő színhez (ikonbuborék, állapotcímke).</summary>
    public static Color Tint(Color color)
    {
        return color.WithAlpha(0.16f);
    }

    public static Color For(ReminderState state)
    {
        return state switch
        {
            ReminderState.Overdue => Danger,
            ReminderState.DueSoon => Warning,
            ReminderState.Ok => Success,
            _ => Neutral,
        };
    }

    public static Color For(EntryType type)
    {
        return type switch
        {
            EntryType.Fuel => Fuel,
            EntryType.Service => Service,
            _ => Odometer,
        };
    }
}
