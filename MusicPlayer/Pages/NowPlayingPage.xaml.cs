using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class NowPlayingPage : ContentPage
{
    private readonly AudioService audioService;

    public NowPlayingPage(AudioService audioService)
    {
        InitializeComponent();

        this.audioService = audioService;

        BindingContext = audioService.CurrentSong;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        BindingContext = audioService.CurrentSong;
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
        await DisplayAlert("Info", "Previous track will be added next.", "OK");
    }

    private async void Next_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Info", "Next track will be added next.", "OK");
    }
}