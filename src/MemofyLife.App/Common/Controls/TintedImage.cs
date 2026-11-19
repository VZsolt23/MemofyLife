using CommunityToolkit.Maui.Behaviors;

namespace MemofyLife.Common.Controls;

/// <summary>
/// Egyszínű SVG ikon tetszőleges színre festve. A <see cref="TintColor"/> XAML-ből
/// DynamicResource-szal is köthető, így sémaváltáskor az ikon is átszíneződik.
/// </summary>
public sealed class TintedImage : Image
{
    public static readonly BindableProperty TintColorProperty = BindableProperty.Create(
        nameof(TintColor),
        typeof(Color),
        typeof(TintedImage),
        Colors.Gray,
        propertyChanged: (b, _, n) => ((TintedImage)b)._tint.TintColor = (Color)n);

    private readonly IconTintColorBehavior _tint = new() { TintColor = Colors.Gray };

    public TintedImage()
    {
        Aspect = Aspect.AspectFit;
        WidthRequest = 20;
        HeightRequest = 20;
        Behaviors.Add(_tint);
    }

    public Color TintColor
    {
        get => (Color)GetValue(TintColorProperty);
        set => SetValue(TintColorProperty, value);
    }
}
