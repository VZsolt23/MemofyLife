using System.Windows.Input;

namespace MemofyLife.Common.Controls;

/// <summary>Üres állapot: ikon, cím, egysoros magyarázat és opcionális gomb.</summary>
public sealed class EmptyState : VerticalStackLayout
{
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(ImageSource),
        typeof(EmptyState),
        propertyChanged: (b, _, n) => ((EmptyState)b)._icon.Icon = (ImageSource?)n);

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(EmptyState),
        Colors.Gray,
        propertyChanged: (b, _, n) => ((EmptyState)b)._icon.Color = (Color)n);

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(EmptyState),
        string.Empty,
        propertyChanged: (b, _, n) => ((EmptyState)b)._title.Text = (string)n);

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message),
        typeof(string),
        typeof(EmptyState),
        string.Empty,
        propertyChanged: (b, _, n) => ((EmptyState)b)._message.Text = (string)n);

    public static readonly BindableProperty ActionTextProperty = BindableProperty.Create(
        nameof(ActionText),
        typeof(string),
        typeof(EmptyState),
        propertyChanged: (b, _, n) =>
        {
            var self = (EmptyState)b;
            self._action.Text = (string?)n;
            self._action.IsVisible = !string.IsNullOrEmpty((string?)n);
        });

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(EmptyState),
        propertyChanged: (b, _, n) => ((EmptyState)b)._action.Command = (ICommand?)n);

    private readonly IconBubble _icon = new() { Size = 64, HorizontalOptions = LayoutOptions.Center };
    private readonly Label _title = new() { HorizontalTextAlignment = TextAlignment.Center };
    private readonly Label _message = new() { HorizontalTextAlignment = TextAlignment.Center };
    private readonly Button _action = new() { IsVisible = false, HorizontalOptions = LayoutOptions.Center };

    public EmptyState()
    {
        Spacing = 12;
        Padding = new Thickness(24);
        VerticalOptions = LayoutOptions.Center;

        _title.Style = (Style)Application.Current!.Resources["TextHeadline"];
        _message.Style = (Style)Application.Current.Resources["TextCaption"];

        Add(_icon);
        Add(_title);
        Add(_message);
        Add(_action);
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

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string? ActionText
    {
        get => (string?)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
}
