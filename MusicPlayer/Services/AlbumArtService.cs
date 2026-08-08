using System.Net.Http.Headers;
using System.Text.Json;

namespace MusicPlayer.Services;
public class AlbumArtService {
    //used to send HTTP requests
    private readonly HttpClient client = new();

    //stores already downloaded covers
    private static readonly Dictionary<string, string?> cache = new();

    public AlbumArtService() {
        //musicBrainz requires a User-Agent
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MusicPlayer", "1.0"));
    }

    //gets album cover URL
    public async Task<string?> GetCoverUrlAsync(string artist, string album, string title) {
        try {
            //remove invalid characters
            artist = Clean(artist);
            album = Clean(album);
            title = Clean(title);

            if (string.IsNullOrWhiteSpace(artist))
                return null;

            //create cache key
            string key = $"{artist}|{album}|{title}".ToLowerInvariant();

            //return cached result if available
            if (cache.TryGetValue(key, out var cached))
                return cached;

            //use only the first artist if there are multiple
            string firstArtist = artist.Split('/')[0].Trim();

            //try to find cover by album
            string? cover = await SearchReleaseAsync(firstArtist, album);

            //if not found, search by song title
            if (cover == null)
                cover = await SearchRecordingAsync(firstArtist, title);

            //save result in cache
            if (cover != null)
                cache[key] = cover;

            return cover;
        }

        catch {
            return null;
        }
    }

    //searches album release in MusicBrainz
    private async Task<string?> SearchReleaseAsync(string artist, string album) {
        if (string.IsNullOrWhiteSpace(album))
            return null;

        try {
            string query = $"artist:\"{artist}\" AND release:\"{album}\"";

            string url = $"https://musicbrainz.org/ws/2/release?query={Uri.EscapeDataString(query)}&fmt=json";

            string json = await client.GetStringAsync(url);

            using JsonDocument doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("releases", out var releases))
                return null;

            foreach (var release in releases.EnumerateArray()) {
                string id = release.GetProperty("id").GetString()!;

                string? image = await TryGetCover(id);

                if (image != null)
                    return image;
            }

            return null;
        }

        catch {
            return null;
        }
    }

    //searches recording if album search failed
    private async Task<string?> SearchRecordingAsync(string artist, string title) {
        if (string.IsNullOrWhiteSpace(title))
            return null;

        try {
            string query = $"artist:\"{artist}\" AND recording:\"{title}\"";

            string url = $"https://musicbrainz.org/ws/2/recording?query={Uri.EscapeDataString(query)}&fmt=json";

            string json = await client.GetStringAsync(url);

            using JsonDocument doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("recordings", out var recordings))
                return null;

            foreach (var recording in recordings.EnumerateArray()) {
                if (!recording.TryGetProperty("releases", out var releases))
                    continue;

                foreach (var release in releases.EnumerateArray()) {
                    string id = release.GetProperty("id").GetString()!;

                    string? image = await TryGetCover(id);

                    if (image != null)
                        return image;
                }
            }

            return null;
        }

        catch {
            return null;
        }
    }

    //gets cover image URL from Cover Art Archive
    private async Task<string?> TryGetCover(string releaseId) {
        try {
            string json = await client.GetStringAsync(
                $"https://coverartarchive.org/release/{releaseId}");

            using JsonDocument doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("images", out var images))
                return null;

            if (images.GetArrayLength() == 0)
                return null;

            return images[0]
                .GetProperty("image")
                .GetString();
        }

        catch {
            //ignore request errors
            return null;
        }
    }

    //removes invalid characters
    private static string Clean(string value) {
        return value.Replace("\0", "").Trim();
    }
}