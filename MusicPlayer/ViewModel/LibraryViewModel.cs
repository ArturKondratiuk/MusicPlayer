using System.Collections.ObjectModel;
using MusicPlayer.Models;

namespace MusicPlayer.ViewModels;

public class LibraryViewModel : BaseViewModel
{
    public ObservableCollection<Song> Songs { get; } = new();

    public void AddSong(Song song)
    {
        Songs.Add(song);
    }
}