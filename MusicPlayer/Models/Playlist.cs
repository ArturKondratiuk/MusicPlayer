using System.Collections.ObjectModel;

namespace MusicPlayer.Models;

public class Playlist {
    //playlist name
    public string Name { get; set; } = "";

    //songs in the playlist
    public ObservableCollection<Song> Songs { get; set; } = new();
}