using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using MemofyLife.Core.Interfaces;
using MemofyLife.Navigation;
using MemofyLife.Features.Home;
using MemofyLife.Features.Arcade.Catalog;
using MemofyLife.Features.Arcade.Games;

namespace MemofyLife
{
    public static class MauiProgram
    {
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

            builder.Services
                .AddHomeFeature()
                .AddArcadeFeature()
                .AddGameFeature();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
