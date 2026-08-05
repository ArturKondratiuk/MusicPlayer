using System.Collections.ObjectModel;
using MusicPlayer.Models;

namespace MusicPlayer.ViewModels;

public class LibraryViewModel : BaseViewModel {
    //collection displayed in the library
    public ObservableCollection<Song> Songs { get; } = new();

    //add song to collection
    public void AddSong(Song song) {
        Songs.Add(song);
    }

    //remove song from collection
    public void RemoveSong(Song song) {
        Songs.Remove(song);
    }
}