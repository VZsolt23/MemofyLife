using MemofyLife.Features.Arcade.Games;
using MemofyLife.Features.Settings;
using MemofyLife.Features.Vehicle;

namespace MemofyLife
{
    public sealed partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        private static void RegisterRoutes()
        {
            VehicleFeature.RegisterRoutes();
            SettingsFeature.RegisterRoutes();
            GameFeature.RegisterRoutes();
        }
    }
}
