using Microsoft.Extensions.DependencyInjection;
using MusicPlayer.Models;
using MusicPlayer.Services;
using MusicPlayer.ViewModels;

namespace MusicPlayer.Pages;

public partial class PlaylistsPage : ContentPage
{
    private readonly PlaylistViewModel viewModel = new();
    private readonly PlaylistService playlistService;

    private readonly AudioService audioService;
    private readonly IServiceProvider serviceProvider;

    public PlaylistsPage(
        AudioService audioService,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        playlistService = new PlaylistService();

        this.audioService = audioService;
        this.serviceProvider = serviceProvider;

        BindingContext = viewModel;

        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        viewModel.Playlists.Clear();

        var playlists = await playlistService.LoadAsync();

        foreach (var playlist in playlists)
            viewModel.AddPlaylist(playlist);
    }

    private async Task SaveAsync()
    {
        await playlistService.SaveAsync(viewModel.Playlists.ToList());
    }

    private async void CreatePlaylist_Clicked(object sender, EventArgs e)
    {
        string name = await DisplayPromptAsync(
            "Playlist",
            "Playlist name:");

        if (string.IsNullOrWhiteSpace(name))
            return;

        viewModel.AddPlaylist(new Playlist
        {
            Name = name
        });

        await SaveAsync();
    }

    private async void DeletePlaylist_Clicked(object sender, EventArgs e)
    {
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
    }

    private async void OpenPlaylist_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Playlist playlist)
            return;

        var page = ActivatorUtilities.CreateInstance<PlaylistDetailsPage>(
            serviceProvider,
            playlist);

        await Navigation.PushAsync(page);
    }

    private async void RenamePlaylist_Clicked(object sender, EventArgs e)
    {
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
}