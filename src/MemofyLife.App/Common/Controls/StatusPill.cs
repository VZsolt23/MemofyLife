using MemofyLife.Common.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace MemofyLife.Common.Controls;

/// <summary>Kis „pirula” címke állapothoz (Rendben / Közeleg / Lejárt).</summary>
public sealed class StatusPill : Border
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(StatusPill),
        string.Empty,
        propertyChanged: (b, _, n) => ((StatusPill)b)._label.Text = (string)n);

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(StatusPill),
        SemanticColors.Neutral,
        propertyChanged: (b, _, _) => ((StatusPill)b).ApplyColor());

    private readonly Label _label = new()
    {
        FontFamily = "AppMedium",
        FontSize = 11,
        VerticalTextAlignment = TextAlignment.Center,
    };

    public StatusPill()
    {
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 10 };
        Padding = new Thickness(10, 3);
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        Content = _label;
        ApplyColor();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    private void ApplyColor()
    {
        BackgroundColor = SemanticColors.Tint(Color);
        _label.TextColor = Color;
    }
}
