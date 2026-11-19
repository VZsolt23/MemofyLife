using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using MemofyLife.Common.Interfaces;
using MemofyLife.Common.Services;
using MemofyLife.Data;
using MemofyLife.Navigation;
using MemofyLife.Features.Home;
using MemofyLife.Features.Vehicle;
using MemofyLife.Features.Arcade.Catalog;
using MemofyLife.Features.Arcade.Games;

namespace MemofyLife
{
    public static class MauiProgram
    {
        private const string DatabaseFileName = "memofylife.db3";

        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton(_ => new AppDatabase(
                Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName)));

            builder.Services
                .AddHomeFeature()
                .AddVehicleFeature()
                .AddArcadeFeature()
                .AddGameFeature();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
