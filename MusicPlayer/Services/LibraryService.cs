using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Models;

namespace MusicPlayer.Services;

public class LibraryService
{
    private readonly JsonService jsonService = new();

    private readonly string filePath =
        Path.Combine(FileSystem.AppDataDirectory, "library.json");

    public async Task SaveLibraryAsync(List<Song> songs)
    {
        await jsonService.SaveAsync(filePath, songs);
    }

    public async Task<List<Song>> LoadLibraryAsync()
    {
        var songs = await jsonService.LoadAsync<List<Song>>(filePath);

        return songs ?? new List<Song>();
    }
}