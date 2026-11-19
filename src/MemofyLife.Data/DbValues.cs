using System.Globalization;

namespace MemofyLife.Data;

/// <summary>
/// Az adatbázisban szövegként tárolt dátumok és időbélyegek egységes formátuma.
/// Naptári dátum: <c>yyyy-MM-dd</c>; időbélyeg: ISO 8601, UTC.
/// </summary>
public static class DbValues
{
    public const string DateFormat = "yyyy-MM-dd";
    public const string TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

    public static string ToDbDate(DateOnly date)
    {
        return date.ToString(DateFormat, CultureInfo.InvariantCulture);
    }

    public static string? ToDbDate(DateOnly? date)
    {
        return date is null ? null : ToDbDate(date.Value);
    }

    public static DateOnly ParseDate(string value)
    {
        return DateOnly.ParseExact(value, DateFormat, CultureInfo.InvariantCulture);
    }

    public static DateOnly? ParseNullableDate(string? value)
    {
        return string.IsNullOrEmpty(value) ? null : ParseDate(value);
    }

    public static string ToDbTimestamp(DateTimeOffset moment)
    {
        return moment.UtcDateTime.ToString(TimestampFormat, CultureInfo.InvariantCulture);
    }
}
