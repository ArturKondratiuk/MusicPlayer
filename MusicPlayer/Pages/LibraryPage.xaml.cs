using Id3;
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

    public LibraryPage(AudioService audioService, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        this.audioService = audioService;
        this.serviceProvider = serviceProvider;

        BindingContext = viewModel;

        Loaded += async (_, _) =>
        {
            await LoadLibraryAsync();
        };
    }

    private async void AddMusicButton_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select a music file"
        });

        if (result == null)
            return;

        Song song = id3Service.ReadSong(result.FullPath);

        viewModel.AddSong(song);
        await libraryService.SaveLibraryAsync(viewModel.Songs.ToList());
    }
    private async Task LoadLibraryAsync()
    {
        viewModel.Songs.Clear();
        var songs = await libraryService.LoadLibraryAsync();

        foreach (var song in songs)
        {
            viewModel.AddSong(song);
        }
    }
    private async void PlaySong_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Song song)
            return;

        audioService.Play(song);

        var page = serviceProvider.GetRequiredService<NowPlayingPage>();

        await Navigation.PushAsync(page);
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

        viewModel.RemoveSong(song);

        await libraryService.SaveLibraryAsync(viewModel.Songs.ToList());
    }
}