using MusicPlayer.Models;
using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class PlaylistDetailsPage : ContentPage {
    private readonly Playlist playlist;

    private readonly AudioService audioService;
    private readonly IServiceProvider serviceProvider;

    public PlaylistDetailsPage(Playlist playlist, AudioService audioService, IServiceProvider serviceProvider) {
        InitializeComponent();

        this.playlist = playlist;
        this.audioService = audioService;
        this.serviceProvider = serviceProvider;

        //show playlist name
        PlaylistNameLabel.Text = playlist.Name;

        //display songs
        SongsCollectionView.ItemsSource = playlist.Songs;

        UpdateEmptyState();
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        //refresh playlist title
        PlaylistNameLabel.Text = playlist.Name;

        UpdateEmptyState();

        //small animation
        Content.Opacity = 0;
        Content.TranslationY = 20;

        await Task.WhenAll(Content.FadeTo(1, 220), Content.TranslateTo(0, 0, 220, Easing.CubicOut));
    }

    //open page to add songs
    private async void AddSongs_Clicked(object sender, EventArgs e) {
        await Navigation.PushAsync(new AddSongsPage(playlist));

        //refresh page after returning
        UpdateEmptyState();

        await SavePlaylist();
    }

    //remove selected song
    private async void DeleteSong_Clicked(object sender, EventArgs e) {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Song song)
            return;

        playlist.Songs.Remove(song);

        UpdateEmptyState();

        await SavePlaylist();
    }

    //play selected song
    private async void SongsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (e.CurrentSelection.FirstOrDefault() is not Song song)
            return;

        //set playlist for next/previous buttons
        audioService.SetPlaylist(playlist.Songs.ToList());

        await audioService.Play(song);

        //remove selection highlight
        SongsCollectionView.SelectedItem = null;

        await Navigation.PushAsync(
            serviceProvider.GetRequiredService<NowPlayingPage>());
    }

    //play the playlist from the first song
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

        await Navigation.PushAsync(
            serviceProvider.GetRequiredService<NowPlayingPage>());
    }

    //save playlist changes
    private async Task SavePlaylist() {
        var service = new PlaylistService();

        var playlists = await service.LoadAsync();

        int index = playlists.FindIndex(p => p.Name == playlist.Name);

        if (index != -1)
            playlists[index] = playlist;

        await service.SaveAsync(playlists);
    }

    //show or hide empty state
    private void UpdateEmptyState() {
        bool empty = playlist.Songs.Count == 0;

        EmptyState.IsVisible = empty;

        SongsCollectionView.IsVisible = !empty;
        SongsLabel.IsVisible = !empty;
        AddButton.IsVisible = !empty;
        PlayButton.IsVisible = !empty;
    }
}