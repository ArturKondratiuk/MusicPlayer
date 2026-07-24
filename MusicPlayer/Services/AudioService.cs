using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Maui.Audio;
using MusicPlayer.Models;

namespace MusicPlayer.Services;

public class AudioService
{
    private IAudioPlayer? player;

    var test = player.

    public event Action? PlaybackUpdated;

    public Song? CurrentSong { get; private set; }

    public List<Song> Playlist { get; private set; } = new();

    public int CurrentIndex { get; private set; } = -1;

    public event Action? SongChanged;

    public bool IsPlaying => player?.IsPlaying ?? false;

    public double Position => player?.CurrentPosition ?? 0;

    public double Duration => player?.Duration ?? 1;

    public string PositionText =>
        TimeSpan.FromSeconds(Position).ToString(@"mm\:ss");

    public string DurationText =>
        TimeSpan.FromSeconds(Duration).ToString(@"mm\:ss");

    public void Play(Song song)
    {
        Stop();

        CurrentSong = song;

        CurrentIndex = Playlist.IndexOf(song);

        var stream = File.OpenRead(song.FilePath);

        player = AudioManager.Current.CreatePlayer(stream);

        player.Play();

        Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
        {
            if (player == null)
                return false;

            PlaybackUpdated?.Invoke();

            return player.IsPlaying;
        });

        SongChanged?.Invoke();

    }

    public void Update()
    {
        PlaybackUpdated?.Invoke();
    }

    public void SetPlaylist(List<Song> songs)
    {
        Playlist = songs;
    }

    public void Pause()
    {
        player?.Pause();
    }

    public void Resume()
    {
        player?.Play();
    }

    public void Stop()
    {
        if (player == null)
            return;

        player.Stop();
        player.Dispose();
        player = null;
    }

    public void TogglePlayPause()
    {
        if (player == null)
            return;

        if (player.IsPlaying)
            player.Pause();
        else
            player.Play();
    }

    public void Next()
    {
        if (Playlist.Count == 0)
            return;

        if (CurrentIndex < Playlist.Count - 1)
        {
            Play(Playlist[CurrentIndex + 1]);
        }
    }

    public void Previous()
    {
        if (Playlist.Count == 0)
            return;

        if (CurrentIndex > 0)
        {
            Play(Playlist[CurrentIndex - 1]);
        }
    }
}