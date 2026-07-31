using MusicPlayer.Models;
using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class AddSongsPage : ContentPage
{
    private readonly Playlist playlist;

    private readonly LibraryService libraryService = new();

    private List<Song> library = new();

    private readonly List<Song> selectedSongs = new();

    public AddSongsPage(Playlist playlist)
    {
        InitializeComponent();

        this.playlist = playlist;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        library = await libraryService.LoadLibraryAsync();

        library = library
            .Where(song =>
                !playlist.Songs.Any(p => p.FilePath == song.FilePath))
            .ToList();

        LibraryView.ItemsSource = library;
    }

    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        string text = e.NewTextValue?.Trim().ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(text))
        {
            LibraryView.ItemsSource = library;
            return;
        }

        LibraryView.ItemsSource = library.Where(song =>
            (song.Title?.ToLower().Contains(text) ?? false) ||
            (song.Artist?.ToLower().Contains(text) ?? false) ||
            (song.Album?.ToLower().Contains(text) ?? false))
            .ToList();
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        foreach (var song in selectedSongs)
        {
            if (!playlist.Songs.Any(s => s.FilePath == song.FilePath))
                playlist.Songs.Add(song);
        }

        await Navigation.PopAsync();
    }

    private void SelectSong_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Song song)
            return;

        if (selectedSongs.Contains(song))
        {
            selectedSongs.Remove(song);

            button.Text = "+";
            button.BackgroundColor = Colors.LightGray;
            button.TextColor = Colors.Black;
        }
        else
        {
            selectedSongs.Add(song);

            button.Text = "✓";
            button.BackgroundColor = Colors.Orange;
            button.TextColor = Colors.White;
        }
    }
}