using MemofyLife.Common.Interfaces;

namespace MemofyLife.Common.Services;

public sealed class DialogService : IDialogService
{
    public Task AlertAsync(string title, string message)
    {
        return Shell.Current.DisplayAlertAsync(title, message, "OK");
    }

    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
    {
        return Shell.Current.DisplayAlertAsync(title, message, accept, cancel);
    }
}
