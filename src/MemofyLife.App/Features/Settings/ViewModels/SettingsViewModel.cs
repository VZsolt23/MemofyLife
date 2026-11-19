using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemofyLife.Common.Theming;
using MemofyLife.Common.ViewModels;

namespace MemofyLife.Features.Settings.ViewModels;

public sealed partial class SettingsViewModel : BaseViewModel
{
    private readonly ThemeService _theme;

    public SettingsViewModel(ThemeService theme)
    {
        _theme = theme;
        Title = "Beállítások";

        Schemes = ColorSchemes.All.Select(s => new SchemeOptionViewModel(s)).ToList();
        Modes =
        [
            new ModeOptionViewModel(ThemeMode.System, "Rendszer", "mobile_screen_solid_full.png"),
            new ModeOptionViewModel(ThemeMode.Light, "Világos", "sun_solid_full.png"),
            new ModeOptionViewModel(ThemeMode.Dark, "Sötét", "moon_solid_full.png"),
        ];

        Refresh();
    }

    public IReadOnlyList<SchemeOptionViewModel> Schemes { get; }

    public IReadOnlyList<ModeOptionViewModel> Modes { get; }

    [RelayCommand]
    private void SelectScheme(SchemeOptionViewModel option)
    {
        _theme.SetScheme(option.Id);
        Refresh();
    }

    [RelayCommand]
    private void SelectMode(ModeOptionViewModel option)
    {
        _theme.SetMode(option.Mode);
        Refresh();
    }

    // A színminták a mód szerinti palettát mutatják, ezért módváltás után is frissülnek.
    private void Refresh()
    {
        bool dark = _theme.IsDark;

        foreach (SchemeOptionViewModel scheme in Schemes)
        {
            scheme.Update(dark, scheme.Id == _theme.Scheme);
        }

        foreach (ModeOptionViewModel mode in Modes)
        {
            mode.IsSelected = mode.Mode == _theme.Mode;
        }
    }
}

public sealed partial class SchemeOptionViewModel : ObservableObject
{
    private readonly ColorSchemeDefinition _definition;

    [ObservableProperty]
    private Color swatch = Colors.Transparent;

    [ObservableProperty]
    private Color swatchContainer = Colors.Transparent;

    [ObservableProperty]
    private bool isSelected;

    public SchemeOptionViewModel(ColorSchemeDefinition definition)
    {
        _definition = definition;
    }

    public ColorSchemeId Id => _definition.Id;

    public string Name => _definition.Name;

    public void Update(bool dark, bool selected)
    {
        SchemePalette palette = dark ? _definition.Dark : _definition.Light;
        Swatch = palette.Primary;
        SwatchContainer = palette.Container;
        IsSelected = selected;
    }
}

public sealed partial class ModeOptionViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isSelected;

    public ModeOptionViewModel(ThemeMode mode, string name, string icon)
    {
        Mode = mode;
        Name = name;
        Icon = icon;
    }

    public ThemeMode Mode { get; }

    public string Name { get; }

    public ImageSource Icon { get; }
}
