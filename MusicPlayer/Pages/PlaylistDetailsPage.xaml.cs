using Microsoft.Extensions.DependencyInjection;
using MusicPlayer.Models;
using MusicPlayer.Services;
using System.Linq;

namespace MusicPlayer.Pages;

public partial class PlaylistDetailsPage : ContentPage
{
    private readonly Playlist playlist;
    private readonly AudioService audioService;
    private readonly IServiceProvider serviceProvider;

    public PlaylistDetailsPage(
        Playlist playlist,
        AudioService audioService,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        this.playlist = playlist;
        this.audioService = audioService;
        this.serviceProvider = serviceProvider;

        PlaylistNameLabel.Text = playlist.Name;

        SongsCollectionView.ItemsSource = playlist.Songs;
    }

    private async void AddSongs_Clicked(object sender, EventArgs e)
    {
        var library = await new LibraryService().LoadLibraryAsync();

        string action = await DisplayActionSheet(
            "Choose song",
            "Cancel",
            null,
            library.Select(s => s.Title).ToArray());

        if (action == "Cancel")
            return;

        Song? song = library.FirstOrDefault(s => s.Title == action);

        if (song == null)
            return;

        if (playlist.Songs.Any(s => s.FilePath == song.FilePath))
            return;

        playlist.Songs.Add(song);

        SongsCollectionView.ItemsSource = null;
        SongsCollectionView.ItemsSource = playlist.Songs;

        await new PlaylistService().SaveAsync(
            await LoadPlaylistsAndReplaceCurrent());
    }

    private async void DeleteSong_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Song song)
            return;

        playlist.Songs.Remove(song);

        SongsCollectionView.ItemsSource = null;
        SongsCollectionView.ItemsSource = playlist.Songs;

        await new PlaylistService().SaveAsync(
            await LoadPlaylistsAndReplaceCurrent());
    }

    private async void SongsCollectionView_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Song song)
            return;

        audioService.SetPlaylist(playlist.Songs.ToList());

        await audioService.Play(song);

        await Navigation.PushAsync(
            serviceProvider.GetRequiredService<NowPlayingPage>());

        SongsCollectionView.SelectedItem = null;
    }

    private async Task<List<Playlist>> LoadPlaylistsAndReplaceCurrent()
    {
        var service = new PlaylistService();

        var playlists = await service.LoadAsync();

        int index = playlists.FindIndex(p => p.Name == playlist.Name);

        if (index != -1)
            playlists[index] = playlist;

        return playlists;
    }

    private async void PlayPlaylist_Clicked(object sender, EventArgs e)
    {
        if (playlist.Songs.Count == 0)
        {
            await DisplayAlert(
                "Playlist",
                "Playlist is empty.",
                "OK");

            return;
        }

        audioService.SetPlaylist(playlist.Songs.ToList());

        await audioService.Play(playlist.Songs[0]);

        await Navigation.PushAsync(
            serviceProvider.GetRequiredService<NowPlayingPage>());
    }
}