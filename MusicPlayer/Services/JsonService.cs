using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace MusicPlayer.Services;
public class JsonService
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true
    };

    public async Task SaveAsync<T>(string filePath, T data)
    {
        string json = JsonSerializer.Serialize(data, options);

        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<T?> LoadAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        try
        {
            string json = await File.ReadAllTextAsync(filePath);

            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }
}