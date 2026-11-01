using MemofyLife.Features.Arcade.Games;

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
            GameFeature.RegisterRoutes();
        }
    }
}
