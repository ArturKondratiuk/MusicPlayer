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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        settings = await settingsService.LoadAsync();

        DarkModeSwitch.IsToggled = settings.DarkMode;

        ConfirmDeleteSwitch.IsToggled = settings.ConfirmDelete;

        AlbumArtSwitch.IsToggled = settings.DownloadAlbumArt;

        RememberShuffleSwitch.IsToggled = settings.RememberShuffle;

        RememberRepeatSwitch.IsToggled = settings.RememberRepeat;

        SortPicker.SelectedItem = settings.DefaultSort;

        ApplyTheme();

        Content.Opacity = 0;
        Content.TranslationY = 20;

        await Task.WhenAll(
            Content.FadeTo(1, 220),
            Content.TranslateTo(0, 0, 220, Easing.CubicOut));
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
        settings.DarkMode = DarkModeSwitch.IsToggled;

        settings.ConfirmDelete = ConfirmDeleteSwitch.IsToggled;

        settings.DownloadAlbumArt = AlbumArtSwitch.IsToggled;

        settings.RememberShuffle = RememberShuffleSwitch.IsToggled;

        settings.RememberRepeat = RememberRepeatSwitch.IsToggled;

        if (!settings.RememberShuffle)
            settings.ShuffleEnabled = false;

        if (!settings.RememberRepeat)
            settings.RepeatModeValue = 0;

        settings.DefaultSort =
            SortPicker.SelectedItem?.ToString() ?? "Title";

        await settingsService.SaveAsync(settings);

        await DisplayAlert(
            "Settings",
            "Settings saved.",
            "OK");
    }
}