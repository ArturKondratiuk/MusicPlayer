using MusicPlayer.Models;
using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public partial class AddSongsPage : ContentPage
{
    //Playlist that will receive new songs
    private readonly Playlist playlist;

    //service for loading the music library
    private readonly LibraryService libraryService = new();

    //songs that can be added
    private List<Song> library = new();

    //songs selected by the user
    private readonly List<Song> selectedSongs = new();

    public AddSongsPage(Playlist playlist) {
        InitializeComponent();

        this.playlist = playlist;
    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        //load all songs from the library
        var allSongs = await libraryService.LoadLibraryAsync();

        //show a message if the library is empty
        if (allSongs.Count == 0) {
            SearchBar.IsVisible = false;
            SaveButton.IsVisible = false;
            SongsCollection.IsVisible = false;

            EmptyState.IsVisible = false;
            LibraryEmptyState.IsVisible = true;

            return;
        }

        SearchBar.IsVisible = true;
        SaveButton.IsVisible = true;
        LibraryEmptyState.IsVisible = false;

        //remove songs that are already in this playlist
        library = allSongs.Where(song => !playlist.Songs.Any(p => p.FilePath == song.FilePath)).ToList();

        //display available songs
        SongsCollection.ItemsSource = library;

        //show a message if there are no songs left to add
        if (library.Count == 0) {
            ShowEmptyState(
                "✅",
                "All songs added",
                "Every song from your library is already in this playlist.");
        }

        else {
            HideEmptyState();
        }

        //small animation
        Content.Opacity = 0;
        Content.TranslationY = 20;

        await Task.WhenAll(Content.FadeTo(1, 220), Content.TranslateTo(0, 0, 220, Easing.CubicOut));
    }

    //filter songs while typing
    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e) {
        string text = e.NewTextValue?.Trim().ToLower() ?? "";

        IEnumerable<Song> filtered;

        //show all songs if the search box is empty
        if (string.IsNullOrWhiteSpace(text)) {
            filtered = library;
        }

        else {
            //search by title, artist or album
            filtered = library.Where(song =>
                (song.Title?.ToLower().Contains(text) ?? false) ||
                (song.Artist?.ToLower().Contains(text) ?? false) ||
                (song.Album?.ToLower().Contains(text) ?? false));
        }

        var result = filtered.ToList();

        SongsCollection.ItemsSource = result;

        //show message if nothing was found
        if (result.Count == 0) {
            ShowEmptyState(
                "🔍",
                "No songs found",
                "Try another search.");
        }

        else {
            HideEmptyState();
        }
    }

    //select or unselect a song
    private void SelectSong_Clicked(object sender, EventArgs e) {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Song song)
            return;

        if (selectedSongs.Contains(song)) {
            //remove from selection
            selectedSongs.Remove(song);

            button.Text = "+";
            button.BackgroundColor = Color.FromArgb("#EEEEEE");
            button.TextColor = Colors.Black;
        }

        else {
            //add to selection
            selectedSongs.Add(song);

            button.Text = "✓";
            button.BackgroundColor = Colors.Orange;
            button.TextColor = Colors.White;
        }
    }

    //add selected songs to the playlist
    private async void Save_Clicked(object sender, EventArgs e) {
        foreach (var song in selectedSongs) {
            //skip duplicates
            if (!playlist.Songs.Any(s => s.FilePath == song.FilePath))
                playlist.Songs.Add(song);
        }

        //return to the previous page
        await Navigation.PopAsync();
    }

    //display an empty state
    private void ShowEmptyState(string icon, string title, string message) {
        LibraryEmptyState.IsVisible = false;
        EmptyState.IsVisible = true;

        SongsCollection.IsVisible = false;

        EmptyIcon.Text = icon;
        EmptyTitle.Text = title;
        EmptyMessage.Text = message;
    }

    //hide the empty state
    private void HideEmptyState() {
        EmptyState.IsVisible = false;
        LibraryEmptyState.IsVisible = false;

        SongsCollection.IsVisible = true;
    }
}