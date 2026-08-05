namespace MusicPlayer.Models;

public class Song {
    //full path to the audio file
    public string FilePath { get; set; } = "";

    //song title
    public string Title { get; set; } = "";

    //artist name
    public string Artist { get; set; } = "";

    //album name
    public string Album { get; set; } = "";

    //album cover URL
    public string? AlbumArt { get; set; }
}