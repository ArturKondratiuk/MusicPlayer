using MusicPlayer.Services;

namespace MusicPlayer;

public partial class App : Application {
    public App() {
        InitializeComponent();

        //apply saved theme on startup
        ApplySavedTheme();
    }

    protected override Window CreateWindow(IActivationState? activationState) {
        //open the main application window
        return new Window(new AppShell());
    }

    //load saved theme from settings
    private async void ApplySavedTheme() {
        var settings = await new SettingsService().LoadAsync();

        UserAppTheme =
            settings.DarkMode
                ? AppTheme.Dark
                : AppTheme.Light;
    }
}