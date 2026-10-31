using MemofyLife.Core.Interfaces;

namespace MemofyLife.Navigation;

public class NavigationService : INavigationService
{
    public Task BackAsync()
    {
        return Shell.Current.GoToAsync("..");
    }

    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        return Shell.Current.GoToAsync(route, parameters);
    }

    public Task GoToTabAsync(string tabRoute)
    {
        return Shell.Current.GoToAsync($"//{tabRoute}");
    }

    public Task OpenPersonEditAsync(int? personId = null)
    {
        var uri = personId is null
            ? Routes.PersonEditPage
            : $"{Routes.PersonEditPage}?{Routes.Params.PersonId}={personId}";

        return Shell.Current.GoToAsync(uri);
    }

    public Task OpenSettingsAsync()
    {
        return Shell.Current.GoToAsync(Routes.SettingsPage);
    }
}
