using MusicPlayer.Models;

namespace MusicPlayer.Services;

public class PlaylistService
{
    //works with json files
    private readonly JsonService jsonService = new();

    //file where playlists are stored
    private readonly string filePath =
        Path.Combine(FileSystem.AppDataDirectory, "playlists.json");

    //save all playlists
    public async Task SaveAsync(List<Playlist> playlists) {
        await jsonService.SaveAsync(filePath, playlists);
    }

    //load playlists from file
    public async Task<List<Playlist>> LoadAsync() {
        var playlists =
            await jsonService.LoadAsync<List<Playlist>>(filePath);

        //return empty list if file doesn't exist
        return playlists ?? new List<Playlist>();
    }
}