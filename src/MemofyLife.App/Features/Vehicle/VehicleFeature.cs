using MemofyLife.Features.Vehicle.ViewModels;
using MemofyLife.Features.Vehicle.Views;
using MemofyLife.Navigation;
using MemofyLife.Vehicle.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MemofyLife.Features.Vehicle;

public static class VehicleFeature
{
    public static IServiceCollection AddVehicleFeature(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<VehicleService>();
        services.AddSingleton<ReminderService>();

        services.AddTransient<VehicleOverviewPage>();
        services.AddTransient<VehicleOverviewViewModel>();
        services.AddTransient<VehicleEditPage>();
        services.AddTransient<VehicleEditViewModel>();
        services.AddTransient<EntryEditPage>();
        services.AddTransient<EntryEditViewModel>();
        services.AddTransient<VehicleLogPage>();
        services.AddTransient<VehicleLogViewModel>();
        services.AddTransient<ReminderEditPage>();
        services.AddTransient<ReminderEditViewModel>();
        return services;
    }

    public static void RegisterRoutes()
    {
        Routing.RegisterRoute(Routes.VehicleEditPage, typeof(VehicleEditPage));
        Routing.RegisterRoute(Routes.EntryEditPage, typeof(EntryEditPage));
        Routing.RegisterRoute(Routes.VehicleLogPage, typeof(VehicleLogPage));
        Routing.RegisterRoute(Routes.ReminderEditPage, typeof(ReminderEditPage));
    }
}
