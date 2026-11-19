#if ANDROID
using Microsoft.Maui.Platform;
#endif

namespace MemofyLife.Common.Theming;

/// <summary>
/// A színséma és a világos/sötét mód kezelése. Az aktuális színeket az alkalmazás
/// erőforrásai közé írja (<c>Primary</c>, <c>Surface</c>, … és <c>…Brush</c> párjaik), így
/// minden <c>DynamicResource</c> hivatkozás újraindítás nélkül frissül.
/// </summary>
public sealed class ThemeService
{
    private const string SchemeKey = "theme.scheme";
    private const string ModeKey = "theme.mode";

    private readonly IPreferences _preferences;
    private Application? _app;

    public ThemeService(IPreferences preferences)
    {
        _preferences = preferences;
        Scheme = (ColorSchemeId)_preferences.Get(SchemeKey, (int)ColorSchemeId.Indigo);
        Mode = (ThemeMode)_preferences.Get(ModeKey, (int)ThemeMode.System);
    }

    public event EventHandler? Changed;

    public ColorSchemeId Scheme { get; private set; }

    public ThemeMode Mode { get; private set; }

    public bool IsDark => _app?.RequestedTheme == AppTheme.Dark;

    /// <summary>Az <see cref="Application"/> konstruktorából, az első oldal előtt hívandó.</summary>
    public void Initialize(Application app)
    {
        _app = app;
        app.UserAppTheme = ToAppTheme(Mode);
        app.RequestedThemeChanged += (_, _) => Apply();
        Apply();
    }

    public void SetScheme(ColorSchemeId scheme)
    {
        Scheme = scheme;
        _preferences.Set(SchemeKey, (int)scheme);
        Apply();
    }

    public void SetMode(ThemeMode mode)
    {
        Mode = mode;
        _preferences.Set(ModeKey, (int)mode);

        if (_app is not null)
        {
            _app.UserAppTheme = ToAppTheme(mode);
        }

        Apply();
    }

    /// <summary>Az Android állapotsorát a háttérhez igazítja (az ablak létrejötte után hívható).</summary>
    public void ApplySystemBars()
    {
#if ANDROID || IOS
        if (!OperatingSystem.IsAndroidVersionAtLeast(23) && !OperatingSystem.IsIOS())
        {
            return;
        }

        try
        {
            NeutralPalette neutral = IsDark ? NeutralPalette.Dark : NeutralPalette.Light;
            CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(neutral.Background);
            CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(IsDark
                ? CommunityToolkit.Maui.Core.StatusBarStyle.LightContent
                : CommunityToolkit.Maui.Core.StatusBarStyle.DarkContent);
#if ANDROID
            ApplyNavigationBar(neutral.Surface, IsDark);
#endif
        }
        catch (Exception)
        {
            // Az Activity még nem él (pl. induláskor); a Window.Created újra meghívja.
        }
#endif
    }

#if ANDROID
    // A rendszer alsó navigációs sávja az alsó fülsávval (Surface) egyezzen.
    private static void ApplyNavigationBar(Color color, bool dark)
    {
        global::Android.Views.Window? window = Platform.CurrentActivity?.Window;

        if (window is null)
        {
            return;
        }

#pragma warning disable CA1422 // Android 15+ alatt elavult, de a régebbi rendszereken kell.
        window.SetNavigationBarColor(color.ToPlatform());
#pragma warning restore CA1422
        AndroidX.Core.View.WindowInsetsControllerCompat? insets =
            AndroidX.Core.View.WindowCompat.GetInsetsController(window, window.DecorView);

        if (insets is not null)
        {
            insets.AppearanceLightNavigationBars = !dark;
        }
    }
#endif

    private void Apply()
    {
        if (_app is null)
        {
            return;
        }

        bool dark = IsDark;
        ColorSchemeDefinition scheme = ColorSchemes.Get(Scheme);
        SchemePalette palette = dark ? scheme.Dark : scheme.Light;
        NeutralPalette neutral = dark ? NeutralPalette.Dark : NeutralPalette.Light;

        ResourceDictionary r = _app.Resources;
        Set(r, "Primary", palette.Primary);
        Set(r, "OnPrimary", palette.OnPrimary);
        Set(r, "PrimaryContainer", palette.Container);
        Set(r, "OnPrimaryContainer", palette.OnContainer);
        Set(r, "Accent", palette.Accent);

        Set(r, "Background", neutral.Background);
        Set(r, "Surface", neutral.Surface);
        Set(r, "SurfaceVariant", neutral.SurfaceVariant);
        Set(r, "OnSurface", neutral.OnSurface);
        Set(r, "OnSurfaceVariant", neutral.OnSurfaceVariant);
        Set(r, "Outline", neutral.Outline);
        Set(r, "OutlineVariant", neutral.OutlineVariant);
        Set(r, "Danger", neutral.Danger);

        ApplySystemBars();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static void Set(ResourceDictionary resources, string key, Color color)
    {
        resources[key] = color;
        resources[$"{key}Brush"] = new SolidColorBrush(color);
    }

    private static AppTheme ToAppTheme(ThemeMode mode)
    {
        return mode switch
        {
            ThemeMode.Light => AppTheme.Light,
            ThemeMode.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };
    }
}
