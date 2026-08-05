using Microsoft.Extensions.Logging;
using MusicPlayer.Pages;
using MusicPlayer.Services;
using Plugin.Maui.Audio;

namespace MusicPlayer;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        //services
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<JsonService>();
        builder.Services.AddSingleton<LibraryService>();
        builder.Services.AddSingleton<PlaylistService>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<AudioService>();

        //pages
        builder.Services.AddTransient<LibraryPage>();
        builder.Services.AddTransient<PlaylistsPage>();
        builder.Services.AddTransient<PlaylistDetailsPage>();
        builder.Services.AddTransient<NowPlayingPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}