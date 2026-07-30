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

    public async Task<string?> GetCoverUrlAsync(
    string artist,
    string album,
    string title)
    {
        artist = Clean(artist);
        album = Clean(album);
        title = Clean(title);

        if (string.IsNullOrWhiteSpace(artist))
            return null;

        string key = $"{artist}|{album}|{title}".ToLowerInvariant();

        if (cache.TryGetValue(key, out var cached))
            return cached;

        string firstArtist = artist.Split('/')[0].Trim();

        string? cover =
            await SearchReleaseAsync(firstArtist, album);

        if (cover == null)
        {
            cover =
                await SearchRecordingAsync(firstArtist, title);
        }

        if (cover != null)
            cache[key] = cover;

        return cover;
    }

    private async Task<string?> SearchReleaseAsync(
    string artist,
    string album)
    {
        if (string.IsNullOrWhiteSpace(album))
            return null;

        string query =
            $"artist:\"{artist}\" AND release:\"{album}\"";

        string url =
            $"https://musicbrainz.org/ws/2/release?query={Uri.EscapeDataString(query)}&fmt=json";

        string json = await client.GetStringAsync(url);

        using JsonDocument doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("releases", out var releases))
            return null;

        foreach (var release in releases.EnumerateArray())
        {
            string id =
                release.GetProperty("id").GetString()!;

            string? image = await TryGetCover(id);

            if (image != null)
                return image;
        }

        return null;
    }

    private async Task<string?> SearchRecordingAsync(
    string artist,
    string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return null;

        string query =
            $"artist:\"{artist}\" AND recording:\"{title}\"";

        string url =
            $"https://musicbrainz.org/ws/2/recording?query={Uri.EscapeDataString(query)}&fmt=json";

        string json = await client.GetStringAsync(url);

        using JsonDocument doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("recordings", out var recordings))
            return null;

        foreach (var recording in recordings.EnumerateArray())
        {
            if (!recording.TryGetProperty("releases", out var releases))
                continue;

            foreach (var release in releases.EnumerateArray())
            {
                string id =
                    release.GetProperty("id").GetString()!;

                string? image = await TryGetCover(id);

                if (image != null)
                    return image;
            }
        }

        return null;
    }

    private async Task<string?> TryGetCover(string releaseId)
    {
        try
        {
            string json =
                await client.GetStringAsync(
                    $"https://coverartarchive.org/release/{releaseId}");

            using JsonDocument doc =
                JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("images", out var images))
                return null;

            if (images.GetArrayLength() == 0)
                return null;

            return images[0]
                .GetProperty("image")
                .GetString();
        }
        catch
        {
            return null;
        }
    }

    private static string Clean(string value)
    {
        return value
            .Replace("\0", "")
            .Trim();
    }
}