using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class NowPlayingPage : ContentPage
{
    private readonly AudioService audioService;

    private readonly SettingsService settingsService = new();

    private bool isDragging;

    public NowPlayingPage(AudioService audioService)
    {
        InitializeComponent();

        this.audioService = audioService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        audioService.PlaybackUpdated -= UpdatePlayer;
        audioService.SongChanged -= UpdateSong;

        audioService.PlaybackUpdated += UpdatePlayer;
        audioService.SongChanged += UpdateSong;

        UpdateSong();
        UpdatePlayer();
        UpdateEmptyState();
        UpdateButtons();

        var settings = await settingsService.LoadAsync();

        if (settings.RememberShuffle)
            audioService.Shuffle = settings.ShuffleEnabled;

        if (settings.RememberRepeat)
            audioService.RepeatMode = settings.RepeatModeValue;

        UpdateButtons();

        VolumeSlider.Value = audioService.GetVolume();

        Content.Opacity = 0;
        Content.TranslationY = 20;

        await Task.WhenAll(
            Content.FadeTo(1, 220),
            Content.TranslateTo(0, 0, 220, Easing.CubicOut));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        audioService.PlaybackUpdated -= UpdatePlayer;
        audioService.SongChanged -= UpdateSong;
    }

    private void UpdateEmptyState()
    {
        bool hasSong = audioService.CurrentSong != null;

        EmptyState.IsVisible = !hasSong;
        PlayerContent.IsVisible = hasSong;
    }

    private void UpdateSong()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateEmptyState();

            var song = audioService.CurrentSong;

            if (song == null)
            {
                Title = "Now Playing";
                BindingContext = null;
                return;
            }

            BindingContext = song;

            if (!string.IsNullOrWhiteSpace(song.AlbumArt))
            {
                AlbumImage.IsVisible = true;
                NoCoverBorder.IsVisible = false;

                AlbumImage.Source =
                    ImageSource.FromUri(new Uri(song.AlbumArt));
            }
            else
            {
                AlbumImage.Source = null;

                AlbumImage.IsVisible = false;
                NoCoverBorder.IsVisible = true;
            }

            Title = $"Now Playing - {song.Title}";
        });
    }

    private void UpdatePlayer()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateEmptyState();

            if (audioService.CurrentSong == null)
                return;

            if (!isDragging)
            {
                ProgressSlider.Maximum = audioService.Duration;
                ProgressSlider.Value = audioService.Position;
            }

            CurrentTimeLabel.Text =
                TimeSpan.FromSeconds(audioService.Position)
                    .ToString(@"mm\:ss");

            DurationLabel.Text =
                TimeSpan.FromSeconds(audioService.Duration)
                    .ToString(@"mm\:ss");

            PlayPauseButton.Text =
                audioService.IsPlaying ? "⏸" : "▶";

            UpdateButtons();
        });
    }

    private void UpdateButtons()
    {
        if (audioService.Shuffle)
        {
            ShuffleButton.BackgroundColor = Colors.Orange;
            ShuffleButton.TextColor = Colors.White;
        }
        else
        {
            ShuffleButton.BackgroundColor = Colors.LightGray;
            ShuffleButton.TextColor = Colors.Black;
        }

        switch (audioService.RepeatMode)
        {
            case 0:
                RepeatButton.Text = "🔁";
                RepeatButton.BackgroundColor = Colors.LightGray;
                RepeatButton.TextColor = Colors.Black;
                break;

            case 1:
                RepeatButton.Text = "🔁";
                RepeatButton.BackgroundColor = Colors.Orange;
                RepeatButton.TextColor = Colors.White;
                break;

            case 2:
                RepeatButton.Text = "🔂";
                RepeatButton.BackgroundColor = Colors.Orange;
                RepeatButton.TextColor = Colors.White;
                break;
        }
    }

    private void ProgressSlider_DragStarted(object sender, EventArgs e)
    {
        isDragging = true;
    }

    private void ProgressSlider_DragCompleted(object sender, EventArgs e)
    {
        isDragging = false;

        audioService.Seek(ProgressSlider.Value);
    }

    private async void PlayPause_Clicked(object sender, EventArgs e)
    {
        await PlayPauseButton.ScaleTo(0.9, 60);
        await PlayPauseButton.ScaleTo(1.1, 60);
        await PlayPauseButton.ScaleTo(1, 60);

        audioService.TogglePlayPause();
    }

    private async void Previous_Clicked(object sender, EventArgs e)
    {
        await audioService.Previous();
    }

    private async void Next_Clicked(object sender, EventArgs e)
    {
        await audioService.Next();
    }

    private void Stop_Clicked(object sender, EventArgs e)
    {
        audioService.Stop();

        UpdateSong();
        UpdatePlayer();
        UpdateEmptyState();
    }

    private async void Shuffle_Clicked(object sender, EventArgs e)
    {
        await ShuffleButton.ScaleTo(0.85, 70);
        await ShuffleButton.ScaleTo(1, 70);

        audioService.Shuffle = !audioService.Shuffle;

        var settings = await settingsService.LoadAsync();

        if (settings.RememberShuffle)
        {
            settings.ShuffleEnabled = audioService.Shuffle;
            await settingsService.SaveAsync(settings);
        }

        UpdateButtons();
    }

    private async void Repeat_Clicked(object sender, EventArgs e)
    {
        await RepeatButton.RotateTo(180, 120);
        await RepeatButton.RotateTo(360, 0);

        audioService.RepeatMode++;

        if (audioService.RepeatMode > 2)
            audioService.RepeatMode = 0;

        var settings = await settingsService.LoadAsync();

        if (settings.RememberRepeat)
        {
            settings.RepeatModeValue = audioService.RepeatMode;
            await settingsService.SaveAsync(settings);
        }

        UpdatePlayer();
    }

    private void VolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        audioService.SetVolume(e.NewValue);
    }
}