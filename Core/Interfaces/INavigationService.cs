namespace MemofyLife.Core.Interfaces;

public interface INavigationService
{
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);
    Task GoToTabAsync(string tabRoute);
    Task OpenPersonEditAsync(int? personId = null);
    Task OpenSettingsAsync();
    Task BackAsync();
}
