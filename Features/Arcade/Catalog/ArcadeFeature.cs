using MemofyLife.Features.Arcade.Catalog.ViewModels;
using MemofyLife.Features.Arcade.Catalog.Views;

namespace MemofyLife.Features.Arcade.Catalog;

public static class ArcadeFeature
{
    public static IServiceCollection AddArcadeFeature(this IServiceCollection services)
    {
        services.AddTransient<ArcadePage>();
        services.AddTransient<ArcadeViewModel>();
        return services;
    }
}
