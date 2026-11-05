using CommunityToolkit.Mvvm.ComponentModel;

namespace MemofyLife.Core.ViewModels
{
    public abstract partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title = string.Empty;
    }
}
