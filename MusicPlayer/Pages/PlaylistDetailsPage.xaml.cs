using Microsoft.Extensions.DependencyInjection;
using MusicPlayer.Models;
using MusicPlayer.Services;

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

        UpdateEmptyState();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        PlaylistNameLabel.Text = playlist.Name;

        UpdateEmptyState();

        Content.Opacity = 0;
        Content.TranslationY = 20;

        await Task.WhenAll(
            Content.FadeTo(1, 220),
            Content.TranslateTo(0, 0, 220, Easing.CubicOut));
    }

    private async void AddSongs_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddSongsPage(playlist));

        // Вернулись назад после добавления

        UpdateEmptyState();

        await SavePlaylist();
    }

    private async void DeleteSong_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Song song)
            return;

        playlist.Songs.Remove(song);

        UpdateEmptyState();

        await SavePlaylist();
    }

    private async void SongsCollectionView_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Song song)
            return;

        audioService.SetPlaylist(playlist.Songs.ToList());

        await audioService.Play(song);

        SongsCollectionView.SelectedItem = null;

        await Navigation.PushAsync(
            serviceProvider.GetRequiredService<NowPlayingPage>());
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

    private async Task SavePlaylist()
    {
        var service = new PlaylistService();

        var playlists = await service.LoadAsync();

        int index = playlists.FindIndex(p => p.Name == playlist.Name);

        if (index != -1)
            playlists[index] = playlist;

        await service.SaveAsync(playlists);
    }

    private void UpdateEmptyState()
    {
        bool empty = playlist.Songs.Count == 0;

        EmptyState.IsVisible = empty;

        SongsCollectionView.IsVisible = !empty;

        SongsLabel.IsVisible = !empty;

        AddButton.IsVisible = !empty;

        PlayButton.IsVisible = !empty;
    }
}