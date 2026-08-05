using MusicPlayer.Models;

namespace MusicPlayer.Services;

public class LibraryService {
    //used to save and load JSON
    private readonly JsonService jsonService = new();

    //path to library file
    private readonly string filePath = Path.Combine(FileSystem.AppDataDirectory, "library.json");

    //saves all songs
    public async Task SaveLibraryAsync(List<Song> songs) {
        await jsonService.SaveAsync(filePath, songs);
    }

    //loads all songs
    public async Task<List<Song>> LoadLibraryAsync() {
        var songs = await jsonService.LoadAsync<List<Song>>(filePath);

        //return empty list if file doesn't exist
        return songs ?? new List<Song>();
    }
}