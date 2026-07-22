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
    private readonly AudioService audioService = new();

    public LibraryPage()
    {
        InitializeComponent();
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
    private async void Songs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Song song)
            return;

        audioService.Play(song.FilePath);

        await Navigation.PushAsync(new NowPlayingPage(song));

        ((CollectionView)sender).SelectedItem = null;
    }
}