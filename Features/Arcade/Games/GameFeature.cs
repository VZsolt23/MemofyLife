using MemofyLife.Features.Arcade.Games.Minesweeper.ViewModels;
using MemofyLife.Features.Arcade.Games.Minesweeper.Views;

namespace MemofyLife.Features.Arcade.Games;

public static class GameFeature
{
    public static IServiceCollection AddGameFeature(this IServiceCollection services)
    {
        services.AddTransient<MinesweeperPage>();
        services.AddTransient<MinesweeperViewModel>();
        return services;
    }
}
