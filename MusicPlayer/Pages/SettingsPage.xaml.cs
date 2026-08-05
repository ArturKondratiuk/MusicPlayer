using MusicPlayer.Models;
using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class SettingsPage : ContentPage {
    //service for loading and saving settings
    private readonly SettingsService settingsService = new();

    //current settings object
    private Settings settings = new();

    public SettingsPage() {
        InitializeComponent();
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        //load saved settings
        settings = await settingsService.LoadAsync();

        //update controls
        DarkModeSwitch.IsToggled = settings.DarkMode;
        ConfirmDeleteSwitch.IsToggled = settings.ConfirmDelete;
        AlbumArtSwitch.IsToggled = settings.DownloadAlbumArt;
        RememberShuffleSwitch.IsToggled = settings.RememberShuffle;
        RememberRepeatSwitch.IsToggled = settings.RememberRepeat;

        SortPicker.SelectedItem = settings.DefaultSort;

        //apply selected theme
        ApplyTheme();

        //small animation
        Content.Opacity = 0;
        Content.TranslationY = 20;

        await Task.WhenAll(Content.FadeTo(1, 220), Content.TranslateTo(0, 0, 220, Easing.CubicOut));
    }

    private void DarkModeSwitch_Toggled(object sender, ToggledEventArgs e) {
        //update theme immediately
        settings.DarkMode = e.Value;

        ApplyTheme();
    }

    //apply selected app theme
    private void ApplyTheme() {
        Application.Current!.UserAppTheme =
            settings.DarkMode
                ? AppTheme.Dark
                : AppTheme.Light;
    }

    private async void Save_Clicked(object sender, EventArgs e) {
        //save values from controls
        settings.DarkMode = DarkModeSwitch.IsToggled;
        settings.ConfirmDelete = ConfirmDeleteSwitch.IsToggled;
        settings.DownloadAlbumArt = AlbumArtSwitch.IsToggled;
        settings.RememberShuffle = RememberShuffleSwitch.IsToggled;
        settings.RememberRepeat = RememberRepeatSwitch.IsToggled;

        //reset playback settings if remembering is disabled
        if (!settings.RememberShuffle)
            settings.ShuffleEnabled = false;

        if (!settings.RememberRepeat)
            settings.RepeatModeValue = 0;

        //save selected sorting
        settings.DefaultSort = SortPicker.SelectedItem?.ToString() ?? "Title";

        //save settings to file
        await settingsService.SaveAsync(settings);

        await DisplayAlert(
            "Settings",
            "Settings saved.",
            "OK");
    }
}