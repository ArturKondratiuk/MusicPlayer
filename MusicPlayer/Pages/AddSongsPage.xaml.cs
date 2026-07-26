using MusicPlayer.Models;
using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class AddSongsPage : ContentPage
{
    private readonly Playlist playlist;
    private readonly LibraryService libraryService = new();

    public AddSongsPage(Playlist playlist)
    {
        InitializeComponent();

        this.playlist = playlist;

        Loaded += async (_, _) =>
        {
            LibraryView.ItemsSource =
                await libraryService.LoadLibraryAsync();
        };
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        foreach (Song song in LibraryView.SelectedItems)
        {
            if (!playlist.Songs.Any(s => s.FilePath == song.FilePath))
            {
                playlist.Songs.Add(song);
            }
        }

        await Navigation.PopAsync();
    }
}