using MusicPlayer.Models;
using MusicPlayer.Services;
using MusicPlayer.ViewModels;

namespace MusicPlayer.Pages;

public partial class PlaylistsPage : ContentPage {
    //stores playlists for the UI
    private readonly PlaylistViewModel viewModel = new();

    //saves and loads playlists
    private readonly PlaylistService playlistService = new();

    //used to play music
    private readonly AudioService audioService;

    //used to create pages with DI
    private readonly IServiceProvider serviceProvider;

    public PlaylistsPage(AudioService audioService, IServiceProvider serviceProvider) {
        InitializeComponent();

        this.audioService = audioService;
        this.serviceProvider = serviceProvider;

        BindingContext = viewModel;
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        //small animation
        Content.Opacity = 0;
        Content.TranslationY = 20;

        await LoadAsync();

        await Task.WhenAll(Content.FadeTo(1, 220), Content.TranslateTo(0, 0, 220, Easing.CubicOut));
    }

    // Load playlists from file
    private async Task LoadAsync() {
        viewModel.Playlists.Clear();

        var playlists = await playlistService.LoadAsync();

        foreach (var playlist in playlists)
            viewModel.AddPlaylist(playlist);

        UpdateEmptyState();

        //animate playlist list
        PlaylistsCollection.Opacity = 0;
        PlaylistsCollection.TranslationY = 20;

        await Task.WhenAll(PlaylistsCollection.FadeTo(1, 220), PlaylistsCollection.TranslateTo(0, 0, 220, Easing.CubicOut));
    }

    //save playlists to file
    private async Task SaveAsync() {
        await playlistService.SaveAsync(viewModel.Playlists.ToList());
    }

    //create a new playlist
    private async void CreatePlaylist_Clicked(object sender, EventArgs e) {
        string? name = await DisplayPromptAsync(
            "Playlist",
            "Playlist name:");

        if (string.IsNullOrWhiteSpace(name))
            return;

        viewModel.AddPlaylist(new Playlist { Name = name.Trim() });

        await SaveAsync();

        UpdateEmptyState();
    }

    //delete selected playlist
    private async void DeletePlaylist_Clicked(object sender, EventArgs e) {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Playlist playlist)
            return;

        bool answer = await DisplayAlert(
            "Delete",
            $"Delete {playlist.Name}?",
            "Delete",
            "Cancel");

        if (!answer)
            return;

        viewModel.RemovePlaylist(playlist);

        await SaveAsync();

        UpdateEmptyState();
    }

    //open playlist page
    private async void OpenPlaylist_Clicked(object sender, EventArgs e) {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Playlist playlist)
            return;

        var page = ActivatorUtilities.CreateInstance<PlaylistDetailsPage>(
            serviceProvider,
            playlist);

        await Navigation.PushAsync(page);
    }

    //rename playlist
    private async void RenamePlaylist_Clicked(object sender, EventArgs e) {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Playlist playlist)
            return;

        string? newName = await DisplayPromptAsync(
            "Rename Playlist",
            "New name:",
            initialValue: playlist.Name);

        if (string.IsNullOrWhiteSpace(newName))
            return;

        playlist.Name = newName.Trim();

        await SaveAsync();

        await LoadAsync();
    }

    //show or hide empty state
    private void UpdateEmptyState() {
        bool empty = viewModel.Playlists.Count == 0;

        EmptyState.IsVisible = empty;
        PlaylistsCollection.IsVisible = !empty;
        TopBar.IsVisible = !empty;
    }
}