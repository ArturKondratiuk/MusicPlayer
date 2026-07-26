using MusicPlayer.Models;
using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsService settingsService = new();

    private Settings settings = new();

    public SettingsPage()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            settings = await settingsService.LoadAsync();

            DarkModeSwitch.IsToggled = settings.DarkMode;

            SortPicker.SelectedItem = settings.DefaultSort;

            ApplyTheme();
        };
    }

    private void DarkModeSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        settings.DarkMode = e.Value;

        ApplyTheme();
    }

    private void ApplyTheme()
    {
        Application.Current!.UserAppTheme =
            settings.DarkMode
            ? AppTheme.Dark
            : AppTheme.Light;
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        settings.DefaultSort =
            SortPicker.SelectedItem?.ToString() ?? "Title";

        await settingsService.SaveAsync(settings);

        await DisplayAlert(
            "Settings",
            "Settings saved.",
            "OK");
    }
}