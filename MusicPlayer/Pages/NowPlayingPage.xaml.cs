using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class NowPlayingPage : ContentPage
{
    private readonly AudioService audioService;

    private bool isDragging = false;

    public NowPlayingPage(AudioService audioService)
    {
        InitializeComponent();

        this.audioService = audioService;

        BindingContext = audioService.CurrentSong;

        audioService.PlaybackUpdated += UpdatePlayer;

    }

    private void UpdatePlayer()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (audioService.CurrentSong == null)
                return;

            BindingContext = audioService.CurrentSong;

            if (!isDragging)
            {
                ProgressSlider.Maximum = audioService.Duration;
                ProgressSlider.Value = audioService.Position;
            }

            CurrentTimeLabel.Text =
                TimeSpan.FromSeconds(audioService.Position).ToString(@"mm\:ss");

            DurationLabel.Text =
                TimeSpan.FromSeconds(audioService.Duration).ToString(@"mm\:ss");
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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        BindingContext = audioService.CurrentSong;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        audioService.PlaybackUpdated -= UpdatePlayer;
    }

    private async void Stop_Clicked(object sender, EventArgs e)
    {
        audioService.Stop();

        await Navigation.PopAsync();
    }

    private void PlayPause_Clicked(object sender, EventArgs e)
    {
        audioService.TogglePlayPause();
    }

    private async void Previous_Clicked(object sender, EventArgs e)
    {
        audioService.Previous();

        BindingContext = audioService.CurrentSong;
    }

    private async void Next_Clicked(object sender, EventArgs e)
    {
        audioService.Next();

        BindingContext = audioService.CurrentSong;
    }
}