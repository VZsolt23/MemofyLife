using MemofyLife.Features.Arcade.Games.Minesweeper.ViewModels;
using MemofyLife.Features.Arcade.Games.Minesweeper.Views;
using MemofyLife.Features.Arcade.Shared;
using MemofyLife.Navigation;

namespace MemofyLife.Features.Arcade.Games.Minesweeper;

public static class MinesweeperFeature
{
    public static IServiceCollection AddMinesweeper(this IServiceCollection services)
    {
        services.AddSingleton<IGame, MinesweeperGame>();
        services.AddTransient<MinesweeperPage>();
        services.AddTransient<MinesweeperViewModel>();
        return services;
    }

    public static void RegisterRoutes()
    {
        Routing.RegisterRoute(Routes.MinesweeperPage, typeof(MinesweeperPage));
    }
}
