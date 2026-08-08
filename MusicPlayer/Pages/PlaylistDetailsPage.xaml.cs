using MusicPlayer.Models;
using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class PlaylistDetailsPage : ContentPage {
    private Playlist playlist;
    private readonly AudioService audioService;
    private readonly IServiceProvider serviceProvider;

    private readonly PlaylistService playlistService = new();

    public PlaylistDetailsPage(Playlist playlist, AudioService audioService, IServiceProvider serviceProvider) {
        InitializeComponent();

        this.playlist = playlist;
        this.audioService = audioService;
        this.serviceProvider = serviceProvider;

        PlaylistNameLabel.Text = playlist.Name;

        SongsCollectionView.ItemsSource = playlist.Songs;

        UpdateEmptyState();
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        await LoadPlaylist();

        Content.Opacity = 0;
        Content.TranslationY = 20;

        await Task.WhenAll(
            Content.FadeTo(1, 220),
            Content.TranslateTo(0, 0, 220, Easing.CubicOut));
    }

    private async Task LoadPlaylist() {
        var playlists = await playlistService.LoadAsync();

        var loadedPlaylist = playlists.FirstOrDefault(
            p => p.Name == playlist.Name);

        if (loadedPlaylist != null)
            playlist = loadedPlaylist;

        PlaylistNameLabel.Text = playlist.Name;

        SongsCollectionView.ItemsSource = null;
        SongsCollectionView.ItemsSource = playlist.Songs;

        UpdateEmptyState();
    }

    private async void AddSongs_Clicked(object sender, EventArgs e) {
        await Navigation.PushAsync(
            new AddSongsPage(playlist));

        await LoadPlaylist();
    }

    private async void DeleteSong_Clicked(object sender, EventArgs e) {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Song song)
            return;

        playlist.Songs.Remove(song);

        await SavePlaylist();

        await LoadPlaylist();
    }

    private async void SongsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (e.CurrentSelection.FirstOrDefault() is not Song song)
            return;

        audioService.SetPlaylist(playlist.Songs.ToList());

        await audioService.Play(song);

        SongsCollectionView.SelectedItem = null;

        await Navigation.PushAsync(
            serviceProvider.GetRequiredService<NowPlayingPage>());
    }

    private async void PlayPlaylist_Clicked(object sender, EventArgs e) {
        if (playlist.Songs.Count == 0) {
            await DisplayAlert(
                "Playlist",
                "Playlist is empty.",
                "OK");

            return;
        }

        audioService.SetPlaylist(playlist.Songs.ToList());

        await audioService.Play(playlist.Songs[0]);

        await Navigation.PushAsync(serviceProvider.GetRequiredService<NowPlayingPage>());
    }

    private async Task SavePlaylist() {
        var playlists = await playlistService.LoadAsync();

        var index = playlists.FindIndex(
            p => p.Name == playlist.Name);

        if (index != -1)
            playlists[index] = playlist;

        await playlistService.SaveAsync(playlists);
    }

    private void UpdateEmptyState() {
        bool empty = playlist.Songs.Count == 0;

        EmptyState.IsVisible = empty;

        SongsCollectionView.IsVisible = !empty;

        SongsLabel.IsVisible = !empty;

        AddButton.IsVisible = !empty;

        PlayButton.IsVisible = !empty;
    }
}