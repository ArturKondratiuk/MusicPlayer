using System.Collections.ObjectModel;
using MusicPlayer.Models;

namespace MusicPlayer.ViewModels;

public class PlaylistViewModel : BaseViewModel {
    //сollection displayed on the playlists page
    public ObservableCollection<Playlist> Playlists { get; } = new();

    //add playlist to collection
    public void AddPlaylist(Playlist playlist) {
        Playlists.Add(playlist);
    }

    //remove playlist from collection
    public void RemovePlaylist(Playlist playlist) {
        Playlists.Remove(playlist);
    }
}