namespace MemofyLife.Common.Interfaces;

public interface IDialogService
{
    Task AlertAsync(string title, string message);

    Task<bool> ConfirmAsync(
        string title,
        string message,
        string accept = "Igen",
        string cancel = "Mégse");
}
