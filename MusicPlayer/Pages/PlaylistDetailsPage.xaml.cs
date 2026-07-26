using MusicPlayer.Models;

namespace MusicPlayer.Pages;

public partial class PlaylistDetailsPage : ContentPage
{
    private readonly Playlist playlist;

    public PlaylistDetailsPage(Playlist playlist)
    {
        InitializeComponent();

        this.playlist = playlist;

        PlaylistNameLabel.Text = playlist.Name;

        SongsCollectionView.ItemsSource = playlist.Songs;
    }

    private async void AddSongs_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(
            new AddSongsPage(playlist));

        SongsCollectionView.ItemsSource = null;
        SongsCollectionView.ItemsSource = playlist.Songs;
    }

    private void DeleteSong_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Song song)
            return;

        playlist.Songs.Remove(song);

        SongsCollectionView.ItemsSource = null;
        SongsCollectionView.ItemsSource = playlist.Songs;
    }
}