using MusicPlayer.Models;

namespace MusicPlayer.Pages;
public partial class NowPlayingPage : ContentPage
{
    private readonly Song song;

    public NowPlayingPage(Song song)
    {
        InitializeComponent();

        this.song = song;

        BindingContext = song;
    }
}