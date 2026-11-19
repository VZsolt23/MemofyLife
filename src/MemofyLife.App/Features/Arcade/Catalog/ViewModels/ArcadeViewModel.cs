using CommunityToolkit.Mvvm.Input;
using MemofyLife.Common.Interfaces;
using MemofyLife.Common.Theming;
using MemofyLife.Common.ViewModels;
using MemofyLife.Features.Arcade.Shared;

namespace MemofyLife.Features.Arcade.Catalog.ViewModels;

public sealed partial class ArcadeViewModel : BaseViewModel
{
    private readonly INavigationService _nav;

    public ArcadeViewModel(IEnumerable<IGame> games, INavigationService nav)
    {
        _nav = nav;
        Games = games.Select(g => new GameCardViewModel(g)).ToList();
        Title = "Játszótér";
    }

    public IReadOnlyList<GameCardViewModel> Games { get; }

    [RelayCommand]
    private Task OpenGameAsync(GameCardViewModel card)
    {
        return _nav.OpenGameAsync(card.Game);
    }

    [RelayCommand]
    private Task OpenVariantAsync(GameVariantItem item)
    {
        return _nav.OpenGameAsync(item.Game, item.Variant.Key);
    }
}

/// <summary>Egy játék kártyája: ikon, név és a nehézségi szintek gombjai.</summary>
public sealed class GameCardViewModel
{
    public GameCardViewModel(IGame game)
    {
        Game = game;
        Variants = game.Variants.Select(v => new GameVariantItem(game, v)).ToList();
    }

    public IGame Game { get; }

    public string DisplayName => Game.DisplayName;

    public ImageSource Icon => Game.Icon;

    public Color Color => SemanticColors.Arcade;

    public IReadOnlyList<GameVariantItem> Variants { get; }

    public bool HasVariants => Variants.Count > 0;
}

public sealed record GameVariantItem(IGame Game, GameVariant Variant)
{
    public string DisplayName => Variant.DisplayName;
}
