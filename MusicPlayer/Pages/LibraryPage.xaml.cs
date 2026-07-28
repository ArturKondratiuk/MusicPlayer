using Microsoft.Maui.Storage;
using MusicPlayer.Models;
using MusicPlayer.Services;
using MusicPlayer.ViewModels;

namespace MusicPlayer.Pages;

public partial class LibraryPage : ContentPage
{
    private readonly LibraryViewModel viewModel = new();
    private readonly LibraryService libraryService = new();
    private readonly Id3Service id3Service = new();

    private readonly AudioService audioService;
    private readonly IServiceProvider serviceProvider;

    private bool loaded;

    public LibraryPage(AudioService audioService, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        this.audioService = audioService;
        this.serviceProvider = serviceProvider;

        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (loaded)
            return;

        loaded = true;

        await LoadLibraryAsync();
    }

    private async Task LoadLibraryAsync()
    {
        viewModel.Songs.Clear();

        var songs = await libraryService.LoadLibraryAsync();

        foreach (var song in songs)
            viewModel.AddSong(song);

        audioService.SetPlaylist(viewModel.Songs.ToList());
    }

    private async void AddMusicButton_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select music"
        });

        if (result == null)
            return;

        var song = id3Service.ReadSong(result.FullPath);

        viewModel.AddSong(song);

        audioService.SetPlaylist(viewModel.Songs.ToList());

        await libraryService.SaveLibraryAsync(viewModel.Songs.ToList());
    }

    private async void PlaySong_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Song song)
            return;

        await audioService.Play(song);

        await Navigation.PushAsync(
            serviceProvider.GetRequiredService<NowPlayingPage>());
    }

    private async void DeleteSong_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Song song)
            return;

        bool answer = await DisplayAlert(
            "Delete Song",
            $"Delete \"{song.Title}\"?",
            "Delete",
            "Cancel");

        if (!answer)
            return;

        if (audioService.CurrentSong == song)
            audioService.Stop();

        viewModel.RemoveSong(song);

        audioService.SetPlaylist(viewModel.Songs.ToList());

        await libraryService.SaveLibraryAsync(viewModel.Songs.ToList());
    }
}