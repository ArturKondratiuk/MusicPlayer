using System.Net.Http.Headers;
using System.Text.Json;

namespace MusicPlayer.Services;

public class AlbumArtService
{
    private readonly HttpClient client = new();

    public AlbumArtService()
    {
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("MusicPlayer", "1.0"));

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "MusicPlayer/1.0");
    }

    public async Task<string?> GetCoverUrlAsync(string artist, string album)
    {
        if (string.IsNullOrWhiteSpace(artist) ||
            string.IsNullOrWhiteSpace(album))
            return null;

        try
        {
            string query =
                $"artist:\"{artist}\" AND release:\"{album}\"";

            string url =
                $"https://musicbrainz.org/ws/2/release?query={Uri.EscapeDataString(query)}&fmt=json";

            string json = await client.GetStringAsync(url);

            using JsonDocument document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("releases", out var releases))
                return null;

            if (releases.GetArrayLength() == 0)
                return null;

            string releaseId =
                releases[0].GetProperty("id").GetString()!;

            string coverJson =
                await client.GetStringAsync(
                    $"https://coverartarchive.org/release/{releaseId}");

            using JsonDocument cover =
                JsonDocument.Parse(coverJson);

            if (!cover.RootElement.TryGetProperty("images", out var images))
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
}