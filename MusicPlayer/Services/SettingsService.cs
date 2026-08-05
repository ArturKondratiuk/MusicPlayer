using MusicPlayer.Models;

namespace MusicPlayer.Services;

public class SettingsService {
    //service for reading and writing json files
    private readonly JsonService jsonService = new();

    //path to settings file
    private readonly string filePath = Path.Combine(FileSystem.AppDataDirectory, "settings.json");

    //loads settings from file
    public async Task<Settings> LoadAsync() {
        var settings = await jsonService.LoadAsync<Settings>(filePath);

        //return default settings if file doesn't exist
        return settings ?? new Settings();
    }

    //saves settings to file
    public async Task SaveAsync(Settings settings) {
        await jsonService.SaveAsync(filePath, settings);
    }
}