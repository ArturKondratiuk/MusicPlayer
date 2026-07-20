using Microsoft.Extensions.Logging;
using MusicPlayer.Pages;
using MusicPlayer.Services;
using MusicPlayer.ViewModels;

namespace MusicPlayer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton<JsonService>();
            builder.Services.AddSingleton<LibraryService>();
            builder.Services.AddSingleton<SettingsService>();

            builder.Services.AddSingleton<LibraryViewModel>();

            builder.Services.AddSingleton<LibraryPage>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
