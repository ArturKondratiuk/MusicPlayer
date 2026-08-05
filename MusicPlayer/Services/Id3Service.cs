using Id3;
using MusicPlayer.Models;

namespace MusicPlayer.Services;

public class Id3Service {
    //read song information from mp3 file
    public Song ReadSong(string filePath) {
        //create song with default values
        Song song = new Song {
            FilePath = filePath,
            Title = Path.GetFileNameWithoutExtension(filePath),
            Artist = "Unknown Artist",
            Album = "Unknown Album"
        };

        try {
            //open mp3 file
            var mp3 = new Mp3(filePath);

            //read ID3v2 tag
            var tag = mp3.GetTag(Id3TagFamily.Version2X);

            if (tag != null) {
                //read title
                if (!string.IsNullOrWhiteSpace(tag.Title))
                    song.Title = Clean(tag.Title);

                //read album
                if (!string.IsNullOrWhiteSpace(tag.Album))
                    song.Album = Clean(tag.Album);

                //read artist
                if (tag.Artists != null)
                    song.Artist = Clean(tag.Artists.ToString());
            }
        }

        catch {
            //ignore invalid metadata
        }

        return song;
    }

    //remove invalid characters (yes twice, they are working between files the need tho)
    private static string Clean(string? value) {
        return value?.Replace("\0", "").Trim() ?? "";
    }
}