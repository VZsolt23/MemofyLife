namespace MemofyLife.Common.Theming;

// A számértékek a beállításokba mentődnek: soha nem szabad megváltoztatni őket.
public enum ColorSchemeId
{
    Indigo = 0,
    Teal = 1,
    Sun = 2,
    Red = 3,
    Sky = 4,
}

public enum ThemeMode
{
    System = 0,
    Light = 1,
    Dark = 2,
}

/// <param name="Primary">Kitöltött gombok, kiemelt kártya háttere.</param>
/// <param name="OnPrimary">Szöveg és ikon a <paramref name="Primary"/> színen.</param>
/// <param name="Container">Halvány, tónusos háttér (pl. másodlagos gomb, kijelölés).</param>
/// <param name="OnContainer">Szöveg a <paramref name="Container"/> színen.</param>
/// <param name="Accent">
/// A séma színe a sima felületen (linkek, kiválasztott fül, ikonok). Világos sémáknál
/// (sárga, égkék) sötétebb, mint a <paramref name="Primary"/>, hogy olvasható maradjon.
/// </param>
public sealed record SchemePalette(
    Color Primary,
    Color OnPrimary,
    Color Container,
    Color OnContainer,
    Color Accent);

public sealed record ColorSchemeDefinition(
    ColorSchemeId Id,
    string Name,
    SchemePalette Light,
    SchemePalette Dark);

public static class ColorSchemes
{
    public static IReadOnlyList<ColorSchemeDefinition> All { get; } =
    [
        new(
            ColorSchemeId.Indigo,
            "Indigó",
            new(Hex("#534AB7"), Hex("#FFFFFF"), Hex("#EEEDFE"), Hex("#26215C"), Hex("#534AB7")),
            new(Hex("#AFA9EC"), Hex("#1B1640"), Hex("#3C3489"), Hex("#EEEDFE"), Hex("#AFA9EC"))),
        new(
            ColorSchemeId.Teal,
            "Teal",
            new(Hex("#0F6E56"), Hex("#FFFFFF"), Hex("#E1F5EE"), Hex("#04342C"), Hex("#0F6E56")),
            new(Hex("#5DCAA5"), Hex("#04342C"), Hex("#085041"), Hex("#E1F5EE"), Hex("#5DCAA5"))),
        new(
            ColorSchemeId.Sun,
            "Napsárga",
            new(Hex("#F5B800"), Hex("#2B2100"), Hex("#FFF4CC"), Hex("#4A3600"), Hex("#8A6500")),
            new(Hex("#FFD25E"), Hex("#2B2100"), Hex("#4A3A00"), Hex("#FFF1C2"), Hex("#FFD25E"))),
        new(
            ColorSchemeId.Red,
            "Piros",
            new(Hex("#C62828"), Hex("#FFFFFF"), Hex("#FDE8E8"), Hex("#5C1010"), Hex("#C62828")),
            new(Hex("#F28B82"), Hex("#3B0A0A"), Hex("#7A1F1F"), Hex("#FDE8E8"), Hex("#F28B82"))),
        new(
            ColorSchemeId.Sky,
            "Égkék",
            new(Hex("#9CBEEC"), Hex("#0E2A4D"), Hex("#E3EEFB"), Hex("#0E2A4D"), Hex("#2F6DB5")),
            new(Hex("#9CBEEC"), Hex("#0E2A4D"), Hex("#1F3A5C"), Hex("#DCE9FA"), Hex("#9CBEEC"))),
    ];

    public static ColorSchemeDefinition Get(ColorSchemeId id)
    {
        return All.FirstOrDefault(s => s.Id == id) ?? All[0];
    }

    private static Color Hex(string value)
    {
        return Color.FromArgb(value);
    }
}

/// <summary>A sémától független, csak világos/sötét szerint változó színek.</summary>
internal sealed record NeutralPalette(
    Color Background,
    Color Surface,
    Color SurfaceVariant,
    Color OnSurface,
    Color OnSurfaceVariant,
    Color Outline,
    Color OutlineVariant,
    Color Danger)
{
    public static NeutralPalette Light { get; } = new(
        Color.FromArgb("#F4F4F7"),
        Color.FromArgb("#FFFFFF"),
        Color.FromArgb("#EEEEF2"),
        Color.FromArgb("#1B1B21"),
        Color.FromArgb("#6B6B76"),
        Color.FromArgb("#C9C9D1"),
        Color.FromArgb("#E4E4EA"),
        Color.FromArgb("#C62828"));

    public static NeutralPalette Dark { get; } = new(
        Color.FromArgb("#121216"),
        Color.FromArgb("#1E1E24"),
        Color.FromArgb("#2A2A32"),
        Color.FromArgb("#EDEDF2"),
        Color.FromArgb("#A0A0AB"),
        Color.FromArgb("#4A4A55"),
        Color.FromArgb("#2F2F38"),
        Color.FromArgb("#F28B82"));
}
