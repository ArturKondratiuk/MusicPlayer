using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class NowPlayingPage : ContentPage
{
    private readonly AudioService audioService;

    public NowPlayingPage(AudioService audioService)
    {
        InitializeComponent();

        BindingContext = audioService.CurrentSong;

        this.audioService = audioService;
    }
}