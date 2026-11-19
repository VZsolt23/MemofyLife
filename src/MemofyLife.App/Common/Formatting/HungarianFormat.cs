using System.Globalization;

namespace MemofyLife.Common.Formatting;

/// <summary>Magyar megjelenítés: „128 450 km”, „12 990 Ft”, „2026. nov. 19.”.</summary>
public static class HungarianFormat
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("hu-HU");

    public static string Number(int value)
    {
        return value.ToString("N0", Culture);
    }

    public static string Km(int km)
    {
        return $"{Number(km)} km";
    }

    public static string Forint(int amount)
    {
        return $"{Number(amount)} Ft";
    }

    public static string Liters(double liters)
    {
        return $"{liters.ToString("0.##", Culture)} l";
    }

    public static string Consumption(double litersPer100Km)
    {
        return $"{litersPer100Km.ToString("0.0", Culture)} l/100 km";
    }

    public static string PricePerLiter(double forintPerLiter)
    {
        return $"{forintPerLiter.ToString("N0", Culture)} Ft/l";
    }

    public static string Date(DateOnly date)
    {
        return date.ToString("yyyy. MMM d.", Culture);
    }

    /// <summary>Év nélkül („nov. 19.”), ahol az évet a környezet (pl. hónap-fejléc) mutatja.</summary>
    public static string ShortDate(DateOnly date)
    {
        return date.ToString("MMM d.", Culture);
    }

    /// <summary>Egész szám szóközös ezres tagolással is („128 450”).</summary>
    public static bool TryParseInt(string? text, out int value)
    {
        return int.TryParse(Strip(text), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>Tizedes szám vesszővel vagy ponttal („38,4” vagy „38.4”).</summary>
    public static bool TryParseDouble(string? text, out double value)
    {
        string normalized = Strip(text).Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>Üres szövegnél null; egyébként a szám, vagy hiba esetén false.</summary>
    public static bool TryParseOptionalInt(string? text, out int? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (TryParseInt(text, out int parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }

    public static string? ToInputText(int? value)
    {
        return value?.ToString(CultureInfo.InvariantCulture);
    }

    public static string? ToInputText(double? value)
    {
        return value?.ToString("0.###", Culture);
    }

    private static string Strip(string? text)
    {
        // Szóköz, nem törő szóköz és keskeny nem törő szóköz (a hu-HU ezres elválasztója).
        return (text ?? string.Empty)
            .Replace(" ", string.Empty)
            .Replace(" ", string.Empty)
            .Replace(" ", string.Empty)
            .Trim();
    }
}
