using MemofyLife.Navigation;

namespace MemofyLife
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        private static void RegisterRoutes()
        {
            //Routing.RegisterRoute(Routes.SettingsPage, typeof(SettingsPage));
            //Routing.RegisterRoute(Routes.PersonEditPage, typeof(PersonEditPage));
            //Routing.RegisterRoute(Routes.VehicleLogPage, typeof(VehicleLogPage));
            //Routing.RegisterRoute(Routes.EntryEditPage, typeof(EntryEditPage));
            //Routing.RegisterRoute(Routes.MineSweeperPage, typeof(MineSweeperPage));
        }
    }
}
