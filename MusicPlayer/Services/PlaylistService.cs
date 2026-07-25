using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Models;

namespace MusicPlayer.Services;

public class PlaylistService
{
    private readonly JsonService jsonService = new();

    private readonly string filePath =
        Path.Combine(FileSystem.AppDataDirectory, "playlists.json");

    public async Task SaveAsync(List<Playlist> playlists)
    {
        await jsonService.SaveAsync(filePath, playlists);
    }

    public async Task<List<Playlist>> LoadAsync()
    {
        var playlists =
            await jsonService.LoadAsync<List<Playlist>>(filePath);

        return playlists ?? new();
    }
}