using MemofyLife.Core.Interfaces;

namespace MemofyLife.Navigation;

public sealed class NavigationService : INavigationService
{
    public Task BackAsync()
    {
        return Shell.Current.GoToAsync("..");
    }

    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        return parameters is null
            ? Shell.Current.GoToAsync(route)
            : Shell.Current.GoToAsync(route, parameters);
    }

    public Task GoToTabAsync(string tabRoute)
    {
        return Shell.Current.GoToAsync($"//{tabRoute}");
    }
}
