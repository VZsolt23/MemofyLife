using MemofyLife.Features.Home.ViewModels;
using MemofyLife.Features.Home.Views;

namespace MemofyLife.Features.Home;

public static class HomeFeature
{
    public static IServiceCollection AddHomeFeature(this IServiceCollection services)
    {
        services.AddTransient<HomePage>();
        services.AddTransient<HomeViewModel>();
        return services;
    }
}
