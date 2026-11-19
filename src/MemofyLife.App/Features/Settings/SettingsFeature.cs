using MemofyLife.Features.Settings.ViewModels;
using MemofyLife.Features.Settings.Views;
using MemofyLife.Navigation;

namespace MemofyLife.Features.Settings;

public static class SettingsFeature
{
    public static IServiceCollection AddSettingsFeature(this IServiceCollection services)
    {
        services.AddTransient<SettingsPage>();
        services.AddTransient<SettingsViewModel>();
        return services;
    }

    public static void RegisterRoutes()
    {
        Routing.RegisterRoute(Routes.SettingsPage, typeof(SettingsPage));
    }
}
