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

        var allSongs = await libraryService.LoadLibraryAsync();

        if (allSongs.Count == 0)
        {
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

        library = allSongs
            .Where(song => !playlist.Songs.Any(p => p.FilePath == song.FilePath))
            .ToList();

        SongsCollection.ItemsSource = library;

        if (library.Count == 0)
        {
            ShowEmptyState(
                "✅",
                "All songs added",
                "Every song from your library is already in this playlist.");
        }
        else
        {
            HideEmptyState();
        }

        Content.Opacity = 0;
        Content.TranslationY = 20;

        await Task.WhenAll(
            Content.FadeTo(1, 220),
            Content.TranslateTo(0, 0, 220, Easing.CubicOut));
    }

    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        string text = e.NewTextValue?.Trim().ToLower() ?? "";

        IEnumerable<Song> filtered;

        if (string.IsNullOrWhiteSpace(text))
        {
            filtered = library;
        }
        else
        {
            filtered = library.Where(song =>
                (song.Title?.ToLower().Contains(text) ?? false) ||
                (song.Artist?.ToLower().Contains(text) ?? false) ||
                (song.Album?.ToLower().Contains(text) ?? false));
        }

        var result = filtered.ToList();

        SongsCollection.ItemsSource = result;

        if (result.Count == 0)
        {
            ShowEmptyState(
                "🔍",
                "No songs found",
                "Try another search.");
        }
        else
        {
            HideEmptyState();
        }
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
            button.BackgroundColor = Color.FromArgb("#EEEEEE");
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

    private async void Save_Clicked(object sender, EventArgs e)
    {
        foreach (Song song in selectedSongs)
        {
            if (!playlist.Songs.Any(s => s.FilePath == song.FilePath))
            {
                playlist.Songs.Add(song);
            }
        }

        await Navigation.PopAsync();
    }

    private void ShowEmptyState(
    string icon,
    string title,
    string message)
    {
        LibraryEmptyState.IsVisible = false;
        EmptyState.IsVisible = true;

        SongsCollection.IsVisible = false;

        EmptyIcon.Text = icon;
        EmptyTitle.Text = title;
        EmptyMessage.Text = message;
    }

    private void HideEmptyState()
    {
        EmptyState.IsVisible = false;
        LibraryEmptyState.IsVisible = false;

        SongsCollection.IsVisible = true;
    }
}