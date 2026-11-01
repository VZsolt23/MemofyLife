using CommunityToolkit.Mvvm.Input;
using MemofyLife.Core.Interfaces;
using MemofyLife.Core.ViewModels;
using MemofyLife.Features.Arcade.Shared;

namespace MemofyLife.Features.Arcade.Catalog.ViewModels;

public sealed partial class ArcadeViewModel : BaseViewModel
{
    private readonly INavigationService _nav;

    public IReadOnlyList<IGame> Games { get; }

    public ArcadeViewModel(IEnumerable<IGame> games, INavigationService nav)
    {
        _nav = nav;
        Games = games.ToList();
        Title = "Játszótér";
    }

    [RelayCommand]
    private Task OpenGameAsync(IGame game)
    {
        return _nav.OpenGameAsync(game);
    }
}
