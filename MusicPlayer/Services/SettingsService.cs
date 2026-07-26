using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Services;

public class SettingsService
{
    private readonly JsonService jsonService = new();

    private readonly string filePath =
        Path.Combine(FileSystem.AppDataDirectory, "settings.json");

    public async Task<Settings> LoadAsync()
    {
        var settings = await jsonService.LoadAsync<Settings>(filePath);

        return settings ?? new Settings();
    }

    public async Task SaveAsync(    Settings settings)
    {
        await jsonService.SaveAsync(filePath, settings);
    }
}