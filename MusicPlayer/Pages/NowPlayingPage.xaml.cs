using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class NowPlayingPage : ContentPage {
    private readonly AudioService audioService;
    private readonly SettingsService settingsService = new();

    //true while user moves the progress slider
    private bool isDragging;

    public NowPlayingPage(AudioService audioService) {
        InitializeComponent();

        this.audioService = audioService;
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        //prevent duplicate subscriptions
        audioService.PlaybackUpdated -= UpdatePlayer;
        audioService.SongChanged -= UpdateSong;

        audioService.PlaybackUpdated += UpdatePlayer;
        audioService.SongChanged += UpdateSong;

        //update UI
        UpdateSong();
        UpdatePlayer();
        UpdateEmptyState();
        UpdateButtons();

        //load saved settings
        var settings = await settingsService.LoadAsync();

        if (settings.RememberShuffle)
            audioService.Shuffle = settings.ShuffleEnabled;

        if (settings.RememberRepeat)
            audioService.RepeatMode = settings.RepeatModeValue;

        UpdateButtons();

        //restore volume
        VolumeSlider.Value = audioService.GetVolume();

        //small animation
        Content.Opacity = 0;
        Content.TranslationY = 20;

        await Task.WhenAll(Content.FadeTo(1, 220), Content.TranslateTo(0, 0, 220, Easing.CubicOut));
    }

    protected override void OnDisappearing() {
        base.OnDisappearing();

        //remove events
        audioService.PlaybackUpdated -= UpdatePlayer;
        audioService.SongChanged -= UpdateSong;
    }

    //show player or empty message
    private void UpdateEmptyState() {
        bool hasSong = audioService.CurrentSong != null;

        EmptyState.IsVisible = !hasSong;
        PlayerContent.IsVisible = hasSong;
    }

    //refresh current song information
    private async void UpdateSong() {
        await MainThread.InvokeOnMainThreadAsync(async () => {
            UpdateEmptyState();

            var song = audioService.CurrentSong;

            if (song == null) {
                Title = "Now Playing";
                BindingContext = null;
                AlbumImage.Source = null;

                return;
            }

            //update bindings
            BindingContext = song;

            //try to load album cover
            if (!string.IsNullOrWhiteSpace(song.AlbumArt)) {
                AlbumImage.IsVisible = true;
                NoCoverBorder.IsVisible = false;

                try {
                    using var http = new HttpClient();

                    var bytes = await http.GetByteArrayAsync(song.AlbumArt);

                    AlbumImage.Source = ImageSource.FromStream(() =>
                        new MemoryStream(bytes));
                }

                catch {
                    //show placeholder if loading fails
                    AlbumImage.Source = null;
                    AlbumImage.IsVisible = false;
                    NoCoverBorder.IsVisible = true;
                }
            }

            else {
                //no album art
                AlbumImage.Source = null;
                AlbumImage.IsVisible = false;
                NoCoverBorder.IsVisible = true;
            }

            Title = $"Now Playing - {song.Title}";
        });
    }

    //refresh playback information
    private void UpdatePlayer() {
        MainThread.BeginInvokeOnMainThread(() => {
            UpdateEmptyState();

            if (audioService.CurrentSong == null)
                return;

            //don't update slider while dragging
            if (!isDragging) {
                ProgressSlider.Maximum = audioService.Duration;
                ProgressSlider.Value = audioService.Position;
            }

            CurrentTimeLabel.Text = TimeSpan.FromSeconds(audioService.Position).ToString(@"mm\:ss");

            DurationLabel.Text = TimeSpan.FromSeconds(audioService.Duration).ToString(@"mm\:ss");

            PlayPauseButton.Text = audioService.IsPlaying ? "⏸" : "▶";

            UpdateButtons();
        });
    }

    //update shuffle and repeat buttons
    private void UpdateButtons() {
        if (audioService.Shuffle) {
            ShuffleButton.BackgroundColor = Colors.Orange;
            ShuffleButton.TextColor = Colors.White;
        }

        else {
            ShuffleButton.BackgroundColor = Colors.LightGray;
            ShuffleButton.TextColor = Colors.Black;
        }

        switch (audioService.RepeatMode) {
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

    //user started moving the slider
    private void ProgressSlider_DragStarted(object sender, EventArgs e) {
        isDragging = true;
    }

    //user finished moving the slider
    private void ProgressSlider_DragCompleted(object sender, EventArgs e) {
        isDragging = false;

        audioService.Seek(ProgressSlider.Value);
    }

    //play or pause
    private async void PlayPause_Clicked(object sender, EventArgs e) {
        //small animation
        await PlayPauseButton.ScaleTo(0.9, 60);
        await PlayPauseButton.ScaleTo(1.1, 60);
        await PlayPauseButton.ScaleTo(1, 60);

        audioService.TogglePlayPause();
    }

    // Previous track
    private async void Previous_Clicked(object sender, EventArgs e) {
        await audioService.Previous();
    }

    //next track
    private async void Next_Clicked(object sender, EventArgs e) {
        await audioService.Next();
    }

    //stop playback
    private void Stop_Clicked(object sender, EventArgs e) {
        audioService.Stop();

        UpdateSong();
        UpdatePlayer();
        UpdateEmptyState();
    }

    //enable or disable shuffle
    private async void Shuffle_Clicked(object sender, EventArgs e) {
        //small animation
        await ShuffleButton.ScaleTo(0.85, 70);
        await ShuffleButton.ScaleTo(1, 70);

        audioService.Shuffle = !audioService.Shuffle;

        var settings = await settingsService.LoadAsync();

        if (settings.RememberShuffle) {
            settings.ShuffleEnabled = audioService.Shuffle;
            await settingsService.SaveAsync(settings);
        }

        UpdateButtons();
    }

    //change repeat mode
    private async void Repeat_Clicked(object sender, EventArgs e) {
        //small animation (funny btw)
        await RepeatButton.RotateTo(180, 120);
        await RepeatButton.RotateTo(360, 0);

        audioService.RepeatMode++;

        if (audioService.RepeatMode > 2)
            audioService.RepeatMode = 0;

        var settings = await settingsService.LoadAsync();

        if (settings.RememberRepeat) {
            settings.RepeatModeValue = audioService.RepeatMode;
            await settingsService.SaveAsync(settings);
        }

        UpdatePlayer();
    }

    //change volume
    private void VolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e) {
        audioService.SetVolume(e.NewValue);
    }
}