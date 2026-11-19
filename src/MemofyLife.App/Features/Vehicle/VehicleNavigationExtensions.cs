using MemofyLife.Common.Interfaces;
using MemofyLife.Navigation;
using MemofyLife.Vehicle.Domain;

namespace MemofyLife.Features.Vehicle;

public static class VehicleNavigationExtensions
{
    public static Task OpenVehicleEditAsync(this INavigationService nav, int vehicleId = 0)
    {
        return nav.GoToAsync(Routes.VehicleEditPage, new Dictionary<string, object>
        {
            [Routes.Params.VehicleId] = vehicleId,
        });
    }

    public static Task OpenNewEntryAsync(this INavigationService nav, EntryType type)
    {
        return nav.GoToAsync(Routes.EntryEditPage, new Dictionary<string, object>
        {
            [Routes.Params.EntryType] = type,
        });
    }

    public static Task OpenEntryAsync(this INavigationService nav, int entryId)
    {
        return nav.GoToAsync(Routes.EntryEditPage, new Dictionary<string, object>
        {
            [Routes.Params.EntryId] = entryId,
        });
    }

    public static Task OpenReminderAsync(this INavigationService nav, int reminderId = 0)
    {
        return nav.GoToAsync(Routes.ReminderEditPage, new Dictionary<string, object>
        {
            [Routes.Params.ReminderId] = reminderId,
        });
    }

    public static Task OpenVehicleLogAsync(this INavigationService nav)
    {
        return nav.GoToAsync(Routes.VehicleLogPage);
    }
}
