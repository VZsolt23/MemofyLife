using CommunityToolkit.Mvvm.ComponentModel;

namespace MemofyLife.Common.ViewModels
{
    public abstract partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title = string.Empty;
    }
}
