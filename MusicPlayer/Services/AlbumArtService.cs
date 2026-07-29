using System.Net.Http.Headers;
using System.Text.Json;

namespace MusicPlayer.Services;

public class AlbumArtService
{
    private readonly HttpClient client = new();

    private static readonly Dictionary<string, string?> cache = new();

    public AlbumArtService()
    {
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("MusicPlayer", "1.0"));
    }

    public async Task<string?> GetCoverUrlAsync(string artist, string album)
    {
        if (string.IsNullOrWhiteSpace(artist) ||
            string.IsNullOrWhiteSpace(album))
            return null;

        string key = $"{artist}|{album}".ToLowerInvariant();

        if (cache.TryGetValue(key, out var cachedUrl))
            return cachedUrl;

        try
        {
            string query = $"artist:\"{artist}\" AND release:\"{album}\"";

            string musicBrainzUrl =
                $"https://musicbrainz.org/ws/2/release?query={Uri.EscapeDataString(query)}&fmt=json";

            string json = await client.GetStringAsync(musicBrainzUrl);

            using JsonDocument doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("releases", out var releases) ||
                releases.GetArrayLength() == 0)
            {
                cache[key] = null;
                return null;
            }

            string releaseId =
                releases[0].GetProperty("id").GetString()!;

            string coverJson =
                await client.GetStringAsync(
                    $"https://coverartarchive.org/release/{releaseId}");

            using JsonDocument coverDoc =
                JsonDocument.Parse(coverJson);

            if (!coverDoc.RootElement.TryGetProperty("images", out var images) ||
                images.GetArrayLength() == 0)
            {
                cache[key] = null;
                return null;
            }

            string? image =
                images[0]
                .GetProperty("image")
                .GetString();

            cache[key] = image;

            return image;
        }
        catch
        {
            cache[key] = null;
            return null;
        }
    }
}