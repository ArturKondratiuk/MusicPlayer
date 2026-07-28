using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class NowPlayingPage : ContentPage
{
    private readonly AudioService audioService;

    private bool isDragging;

    public NowPlayingPage(AudioService audioService)
    {
        InitializeComponent();

        this.audioService = audioService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        audioService.PlaybackUpdated -= UpdatePlayer;
        audioService.SongChanged -= UpdateSong;

        audioService.PlaybackUpdated += UpdatePlayer;
        audioService.SongChanged += UpdateSong;

        UpdateSong();
        UpdatePlayer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        audioService.PlaybackUpdated -= UpdatePlayer;
        audioService.SongChanged -= UpdateSong;
    }

    private void UpdateSong()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var song = audioService.CurrentSong;

            if (song == null)
                return;

            BindingContext = song;

            if (string.IsNullOrWhiteSpace(song.AlbumArt))
            {
                AlbumImage.Source = null;
            }
            else
            {
                AlbumImage.Source = ImageSource.FromUri(new Uri(song.AlbumArt));
            }

            Title = song.Title;
        });
    }

    private void UpdatePlayer()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
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
        });
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

    private void PlayPause_Clicked(object sender, EventArgs e)
    {
        audioService.TogglePlayPause();
    }

    private async void Stop_Clicked(object sender, EventArgs e)
    {
        audioService.Stop();

        await Navigation.PopAsync();
    }

    private async void Previous_Clicked(object sender, EventArgs e)
    {
        await audioService.Previous();
    }

    private async void Next_Clicked(object sender, EventArgs e)
    {
        await audioService.Next();
    }
}