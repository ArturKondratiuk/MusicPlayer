using System.Text.Json;

namespace MusicPlayer.Services;

public class JsonService {
    //JSON formatting options
    private readonly JsonSerializerOptions options = new() {
        WriteIndented = true
    };

    //saves object to JSON file
    public async Task SaveAsync<T>(string filePath, T data) {
        string json = JsonSerializer.Serialize(data, options);

        await File.WriteAllTextAsync(filePath, json);
    }

    //loads object from JSON file
    public async Task<T?> LoadAsync<T>(string filePath) {
        //return default if file does not exist
        if (!File.Exists(filePath))
            return default;

        try {
            string json = await File.ReadAllTextAsync(filePath);

            return JsonSerializer.Deserialize<T>(json);
        }

        catch {
            //return default if JSON is invalid
            return default;
        }
    }
}