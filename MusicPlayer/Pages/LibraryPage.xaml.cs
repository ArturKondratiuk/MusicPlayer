using Microsoft.Maui.Storage;
using MusicPlayer.Models;
using MusicPlayer.Services;
using MusicPlayer.ViewModels;

namespace MusicPlayer.Pages;

public partial class LibraryPage : ContentPage
{
    private readonly LibraryViewModel viewModel = new();
    private readonly LibraryService libraryService = new();
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

        Song song = new Song
        {
            FilePath = result.FullPath,
            Title = Path.GetFileNameWithoutExtension(result.FileName),
            Artist = "Unknown Artist",
            Album = "Unknown Album"
        };

        viewModel.AddSong(song);
        await libraryService.SaveLibraryAsync(viewModel.Songs.ToList());
    }
    private async Task LoadLibraryAsync()
    {
        var songs = await libraryService.LoadLibraryAsync();

        foreach (var song in songs)
        {
            viewModel.AddSong(song);
        }
    }
}