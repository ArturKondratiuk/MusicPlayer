using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MusicPlayer.Models;

namespace MusicPlayer.ViewModels;

public class PlaylistViewModel
{
    public ObservableCollection<Playlist> Playlists { get; } = new();

    public void AddPlaylist(Playlist playlist)
    {
        Playlists.Add(playlist);
    }

    public void RemovePlaylist(Playlist playlist)
    {
        Playlists.Remove(playlist);
    }
}