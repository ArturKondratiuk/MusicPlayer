using MusicPlayer.Services;

namespace MusicPlayer;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        ApplySavedTheme();

        MainPage = new AppShell();
    }

    private async void ApplySavedTheme()
    {
        SettingsService settingsService = new();

        var settings = await settingsService.LoadAsync();

        UserAppTheme = settings.DarkMode
            ? AppTheme.Dark
            : AppTheme.Light;
    }
}