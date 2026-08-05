using MusicPlayer.Models;
using MusicPlayer.Services;
using MusicPlayer.ViewModels;

namespace MusicPlayer.Pages;

public partial class LibraryPage : ContentPage {
    //viewModel for CollectionView
    private readonly LibraryViewModel viewModel = new();

    //services
    private readonly LibraryService libraryService = new();
    private readonly Id3Service id3Service = new();
    private readonly SettingsService settingsService = new();

    //all songs from library
    private List<Song> allSongs = new();

    //shared services
    private readonly AudioService audioService;
    private readonly IServiceProvider serviceProvider;

    public LibraryPage(AudioService audioService, IServiceProvider serviceProvider) {
        InitializeComponent();

        this.audioService = audioService;
        this.serviceProvider = serviceProvider;

        BindingContext = viewModel;
    }

    //load library every time page opens
    protected override async void OnAppearing() {
        base.OnAppearing();

        //small animation
        Content.Opacity = 0;
        Content.TranslationY = 20;

        await LoadLibraryAsync();

        await Task.WhenAll(Content.FadeTo(1, 220), Content.TranslateTo(0, 0, 220, Easing.CubicOut));
    }

    //load songs from saved library
    private async Task LoadLibraryAsync() {
        var settings = await settingsService.LoadAsync();

        allSongs = await libraryService.LoadLibraryAsync();

        //sort songs using saved setting
        allSongs = SortSongs(allSongs, settings.DefaultSort);

        //refresh ViewModel
        viewModel.Songs.Clear();

        foreach (var song in allSongs) {
            viewModel.AddSong(song);
        }

        //update playlist for player
        audioService.SetPlaylist(allSongs);

        //show empty screen if library has no songs
        if (allSongs.Count == 0)
            ShowLibraryEmptyState();

        else
            HideEmptyState();

        //small animation
        SongsCollection.Opacity = 0;
        SongsCollection.TranslationY = 20;

        await Task.WhenAll(SongsCollection.FadeTo(1, 220), SongsCollection.TranslateTo(0, 0, 220, Easing.CubicOut));
    }

    //search songs
    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e) {
        string text = e.NewTextValue?.Trim().ToLower() ?? "";

        viewModel.Songs.Clear();

        IEnumerable<Song> filtered;

        if (string.IsNullOrWhiteSpace(text))
            filtered = allSongs;
        
        else 
            filtered = allSongs.Where(song =>
                (song.Title?.ToLower().Contains(text) ?? false) ||
                (song.Artist?.ToLower().Contains(text) ?? false) ||
                (song.Album?.ToLower().Contains(text) ?? false));

        foreach (var song in filtered)
            viewModel.AddSong(song);

        if (!filtered.Any())
            ShowSearchEmptyState();

        else
            HideEmptyState();

        //player should use filtered list
        audioService.SetPlaylist(viewModel.Songs.ToList());
    }

    //import music files
    private async void AddMusicButton_Clicked(object sender, EventArgs e) {
        var files = await FilePicker.Default.PickMultipleAsync(new PickOptions
        {
            PickerTitle = "Select music files"
        });

        if (files == null)
            return;

        bool added = false;

        int addedCount = 0;
        int skippedCount = 0;

        foreach (var file in files) {
            //skip duplicates
            if (allSongs.Any(s => s.FilePath == file.FullPath)) {
                skippedCount++;
                continue;
            }

            try {
                var song = id3Service.ReadSong(file.FullPath);

                allSongs.Add(song);

                addedCount++;
                added = true;
            }

            catch {
                //ignore broken files
            }
        }

        if (!added)
            return;

        await libraryService.SaveLibraryAsync(allSongs);

        await LoadLibraryAsync();

        await DisplayAlert(
            "Import complete",
            $"{addedCount} song(s) imported\n{skippedCount} skipped",
            "OK");

        //small success animation
        AddMusicButton.BackgroundColor = Colors.Green;

        await Task.Delay(500);

        AddMusicButton.BackgroundColor = Colors.Orange;
    }

    //play selected song
    private async void PlaySong_Clicked(object sender, EventArgs e) {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Song song)
            return;

        await audioService.Play(song);

        await Navigation.PushAsync(
            serviceProvider.GetRequiredService<NowPlayingPage>());
    }

    //delete song from library
    private async void DeleteSong_Clicked(object sender, EventArgs e) {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not Song song)
            return;

        var settings = await settingsService.LoadAsync();

        //ask confirmation if enabled
        if (settings.ConfirmDelete) {
            bool answer = await DisplayAlert(
                "Delete Song",
                $"Delete \"{song.Title}\"?",
                "Delete",
                "Cancel");

            if (!answer)
                return;
        }

        //stop player if this song is playing
        if (audioService.CurrentSong == song)
            audioService.Stop();

        allSongs.Remove(song);

        await libraryService.SaveLibraryAsync(allSongs);

        await LoadLibraryAsync();
    }

    //sort songs
    private List<Song> SortSongs(List<Song> songs, string sort) {
        return sort switch {
            "Artist" => songs
                .OrderBy(s => s.Artist)
                .ThenBy(s => s.Title)
                .ToList(),

            "Album" => songs
                .OrderBy(s => s.Album)
                .ThenBy(s => s.Title)
                .ToList(),

            _ => songs
                .OrderBy(s => s.Title)
                .ToList()
        };
    }

    //choose sorting
    private async void SortButton_Clicked(object sender, EventArgs e) {
        string? result = await DisplayActionSheet(
            "Sort library",
            "Cancel",
            null,
            "Title",
            "Artist",
            "Album");

        if (string.IsNullOrEmpty(result) || result == "Cancel")
            return;

        var settings = await settingsService.LoadAsync();

        settings.DefaultSort = result;

        await settingsService.SaveAsync(settings);

        await LoadLibraryAsync();
    }

    //empty library screen
    private void ShowLibraryEmptyState() {
        SongsCollection.IsVisible = false;
        EmptyState.IsVisible = true;

        TopBar.IsVisible = false;
        SearchBar.IsVisible = false;

        EmptyTitle.Text = "Welcome!";

        EmptyMessage.Text =
            "Your music library is empty.\nImport songs or scan a folder to get started.";

        EmptyButton.Text = "Import Music";
    }

    //empty search result screen
    private void ShowSearchEmptyState() {
        SongsCollection.IsVisible = false;
        EmptyState.IsVisible = true;

        TopBar.IsVisible = true;
        SearchBar.IsVisible = true;

        EmptyTitle.Text = "No songs found";
        EmptyMessage.Text = "Try another search.";

        EmptyButton.Text = "Clear Search";
    }

    //return to normal view
    private void HideEmptyState() {
        SongsCollection.IsVisible = true;
        EmptyState.IsVisible = false;

        TopBar.IsVisible = true;
        SearchBar.IsVisible = true;
    }

    //button in empty state
    private async void EmptyButton_Clicked(object sender, EventArgs e) {
        //clear search if searching
        if (SearchBar.Text?.Length > 0) {
            SearchBar.Text = "";
            return;
        }

        //otherwise import music
        await Task.Yield();

        AddMusicButton_Clicked(sender, e);
    }
}