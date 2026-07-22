using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using MusicPlayer.Services;
using MusicPlayer.ViewModels;
using MusicPlayer.Pages;

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
            builder.Services.AddSingleton<AudioService>();
            builder.Services.AddSingleton<LibraryViewModel>();

            builder.Services.AddTransient<LibraryPage>();
            builder.Services.AddTransient<NowPlayingPage>();
            builder.Services.AddTransient<PlaylistsPage>();
            builder.Services.AddTransient<SettingsPage>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
