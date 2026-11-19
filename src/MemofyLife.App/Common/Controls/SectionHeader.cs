using System.Windows.Input;

namespace MemofyLife.Common.Controls;

/// <summary>Szakaszcím bal oldalon, opcionális szöveges művelet („Összes”) jobb oldalon.</summary>
public sealed class SectionHeader : Grid
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(SectionHeader),
        string.Empty,
        propertyChanged: (b, _, n) => ((SectionHeader)b)._title.Text = (string)n);

    public static readonly BindableProperty ActionTextProperty = BindableProperty.Create(
        nameof(ActionText),
        typeof(string),
        typeof(SectionHeader),
        propertyChanged: (b, _, n) => ((SectionHeader)b).ApplyAction());

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(SectionHeader),
        propertyChanged: (b, _, n) => ((SectionHeader)b)._action.Command = (ICommand?)n);

    private readonly Label _title = new();
    private readonly Button _action = new() { IsVisible = false };

    public SectionHeader()
    {
        ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)];
        Margin = new Thickness(4, 16, 0, 4);
        MinimumHeightRequest = 36;

        _title.Style = (Style)Application.Current!.Resources["TextHeadline"];
        _title.VerticalOptions = LayoutOptions.Center;
        _action.Style = (Style)Application.Current.Resources["TextButton"];
        _action.VerticalOptions = LayoutOptions.Center;

        Children.Add(_title);
        Children.Add(_action);
        SetColumn((IView)_action, 1);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
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

    private void ApplyAction()
    {
        _action.Text = ActionText;
        _action.IsVisible = !string.IsNullOrEmpty(ActionText);
    }
}
