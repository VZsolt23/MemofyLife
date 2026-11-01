using MemofyLife.Core.Interfaces;
using MemofyLife.Features.Arcade.Shared;
using MemofyLife.Navigation;

namespace MemofyLife.Features.Arcade.Catalog;

public static class ArcadeNavigationExtensions
{
    public static Task OpenGameAsync(this INavigationService nav, IGame game, string? variant = null)
    {
        Dictionary<string, object>? parameters = null;

        if (variant is not null)
        {
            parameters = new()
            {
                [Routes.Params.Variant] = variant
            };
        }

        return nav.GoToAsync(game.Route, parameters);
    }
}
