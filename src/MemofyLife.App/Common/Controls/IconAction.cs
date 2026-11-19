using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace MemofyLife.Common.Controls;

/// <summary>Kerek, csak ikonos gomb az oldalfejlécben (pl. szerkesztés, beállítások).</summary>
public sealed class IconAction : Border
{
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(ImageSource),
        typeof(IconAction),
        propertyChanged: (b, _, n) => ((IconAction)b)._image.Source = (ImageSource?)n);

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(IconAction),
        propertyChanged: (b, _, n) => ((IconAction)b)._tap.Command = (ICommand?)n);

    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description),
        typeof(string),
        typeof(IconAction),
        propertyChanged: (b, _, n) => SemanticProperties.SetDescription((IconAction)b, (string?)n));

    private readonly TintedImage _image = new() { WidthRequest = 18, HeightRequest = 18 };
    private readonly TapGestureRecognizer _tap = new();

    public IconAction()
    {
        WidthRequest = 44;
        HeightRequest = 44;
        StrokeThickness = 0;
        StrokeShape = new Ellipse();
        VerticalOptions = LayoutOptions.Center;
        Content = _image;
        GestureRecognizers.Add(_tap);

        SetDynamicResource(BackgroundColorProperty, "Surface");
        _image.SetDynamicResource(TintedImage.TintColorProperty, "OnSurfaceVariant");
    }

    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}
