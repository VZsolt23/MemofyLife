using MemofyLife.Common.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace MemofyLife.Common.Controls;

/// <summary>Lekerekített négyzet halvány háttérrel és színes ikonnal (listasorok elején).</summary>
public sealed class IconBubble : Border
{
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(ImageSource),
        typeof(IconBubble),
        propertyChanged: (b, _, n) => ((IconBubble)b)._image.Source = (ImageSource?)n);

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(IconBubble),
        SemanticColors.Neutral,
        propertyChanged: (b, _, _) => ((IconBubble)b).ApplyColor());

    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size),
        typeof(double),
        typeof(IconBubble),
        40d,
        propertyChanged: (b, _, _) => ((IconBubble)b).ApplySize());

    private readonly TintedImage _image = new();

    public IconBubble()
    {
        StrokeThickness = 0;
        Content = _image;
        VerticalOptions = LayoutOptions.Center;
        HorizontalOptions = LayoutOptions.Start;
        ApplySize();
        ApplyColor();
    }

    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private void ApplyColor()
    {
        BackgroundColor = SemanticColors.Tint(Color);
        _image.TintColor = Color;
    }

    private void ApplySize()
    {
        WidthRequest = Size;
        HeightRequest = Size;
        StrokeShape = new RoundRectangle { CornerRadius = Size * 0.3 };
        _image.WidthRequest = Size * 0.45;
        _image.HeightRequest = Size * 0.45;
    }
}
