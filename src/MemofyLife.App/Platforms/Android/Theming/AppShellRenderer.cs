using Android.Content.Res;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;

namespace MemofyLife.Platforms.Android.Theming;

/// <summary>
/// A Shell alsó sávjának kiválasztott-fül jelzője („pirula”) natív Material-szín, ami nem
/// követné a színsémát. Ez a renderer a séma tónusos színére állítja.
/// </summary>
public sealed class AppShellRenderer : ShellRenderer
{
    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(
        ShellItem shellItem)
    {
        return new AppBottomNavAppearanceTracker(this, shellItem);
    }
}

internal sealed class AppBottomNavAppearanceTracker : ShellBottomNavViewAppearanceTracker
{
    public AppBottomNavAppearanceTracker(IShellContext shellContext, ShellItem shellItem)
        : base(shellContext, shellItem)
    {
    }

    public override void SetAppearance(
        BottomNavigationView bottomView,
        IShellAppearanceElement appearance)
    {
        base.SetAppearance(bottomView, appearance);
        ApplyIndicator(bottomView);
    }

    public override void ResetAppearance(BottomNavigationView bottomView)
    {
        base.ResetAppearance(bottomView);
        ApplyIndicator(bottomView);
    }

    private static void ApplyIndicator(BottomNavigationView bottomView)
    {
        if (Microsoft.Maui.Controls.Application.Current?.Resources.TryGetValue(
                "PrimaryContainer", out object? value) == true
            && value is Color color)
        {
            bottomView.ItemActiveIndicatorColor = ColorStateList.ValueOf(color.ToPlatform());
        }
    }
}
