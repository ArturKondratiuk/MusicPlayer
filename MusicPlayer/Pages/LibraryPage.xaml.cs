using Microsoft.Maui.Storage;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;

namespace MusicPlayer.Pages;

public partial class LibraryPage : ContentPage
{
    private readonly LibraryViewModel viewModel = new();
    public LibraryPage()
    {
        InitializeComponent();
        BindingContext = viewModel;
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
    }
}