using MemofyLife.Features.Arcade.Games.Minesweeper;

namespace MemofyLife.Features.Arcade.Games;

public static class GameFeature
{
    public static IServiceCollection AddGameFeature(this IServiceCollection services)
    {
        return services.AddMinesweeper();
    }

    public static void RegisterRoutes()
    {
        MinesweeperFeature.RegisterRoutes();
    }
}
