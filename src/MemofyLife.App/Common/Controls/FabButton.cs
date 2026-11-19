using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace MemofyLife.Common.Controls;

/// <summary>Lebegő, kerekített négyzet gomb ikonnal (pl. „+” új bejegyzéshez).</summary>
public sealed class FabButton : Border
{
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(ImageSource),
        typeof(FabButton),
        propertyChanged: (b, _, n) => ((FabButton)b)._image.Source = (ImageSource?)n);

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(FabButton),
        propertyChanged: (b, _, n) => ((FabButton)b)._tap.Command = (ICommand?)n);

    private readonly TintedImage _image = new() { WidthRequest = 22, HeightRequest = 22 };
    private readonly TapGestureRecognizer _tap = new();

    public FabButton()
    {
        WidthRequest = 56;
        HeightRequest = 56;
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 18 };
        HorizontalOptions = LayoutOptions.End;
        VerticalOptions = LayoutOptions.End;
        Margin = new Thickness(0, 0, 16, 16);
        Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.18f, Radius = 12, Offset = new Point(0, 4) };
        Content = _image;
        GestureRecognizers.Add(_tap);
        SemanticProperties.SetDescription(this, "Új");

        SetDynamicResource(BackgroundColorProperty, "Primary");
        _image.SetDynamicResource(TintedImage.TintColorProperty, "OnPrimary");
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
}
