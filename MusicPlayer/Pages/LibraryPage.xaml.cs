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
    private readonly SettingsService settingsService = new();

    private List<Song> allSongs = new();

    private readonly AudioService audioService;
    private readonly IServiceProvider serviceProvider;

    public LibraryPage(
        AudioService audioService,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        this.audioService = audioService;
        this.serviceProvider = serviceProvider;

        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadLibraryAsync();
    }

    private async Task LoadLibraryAsync()
    {
        var settings = await settingsService.LoadAsync();

        allSongs = await libraryService.LoadLibraryAsync();

        allSongs = SortSongs(allSongs, settings.DefaultSort);

        viewModel.Songs.Clear();

        foreach (var song in allSongs)
            viewModel.AddSong(song);

        audioService.SetPlaylist(allSongs);
    }

    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        string text = e.NewTextValue?.Trim().ToLower() ?? "";

        viewModel.Songs.Clear();

        IEnumerable<Song> filtered;

        if (string.IsNullOrWhiteSpace(text))
        {
            filtered = allSongs;
        }
        else
        {
            filtered = allSongs.Where(song =>
                (song.Title?.ToLower().Contains(text) ?? false) ||
                (song.Artist?.ToLower().Contains(text) ?? false) ||
                (song.Album?.ToLower().Contains(text) ?? false));
        }

        foreach (var song in filtered)
            viewModel.AddSong(song);

        audioService.SetPlaylist(viewModel.Songs.ToList());
    }

    private async void AddMusicButton_Clicked(object sender, EventArgs e)
    {
        var files = await FilePicker.Default.PickMultipleAsync(new PickOptions
        {
            PickerTitle = "Select music files"
        });

        if (files == null)
            return;

        bool added = false;

        foreach (var file in files)
        {
            if (allSongs.Any(s => s.FilePath == file.FullPath))
                continue;

            try
            {
                var song = id3Service.ReadSong(file.FullPath);  

                allSongs.Add(song);

                added = true;
            }
            catch
            {
            }
        }

        if (!added)
            return;

        await libraryService.SaveLibraryAsync(allSongs);

        await LoadLibraryAsync();
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

        allSongs.Remove(song);

        await libraryService.SaveLibraryAsync(allSongs);

        await LoadLibraryAsync();
    }

    private List<Song> SortSongs(List<Song> songs, string sort)
    {
        return sort switch
        {
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

    private async void SortButton_Clicked(object sender, EventArgs e)
    {
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
}