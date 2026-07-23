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

    public Song? CurrentSong { get; private set; }

    public event Action? SongChanged;

    public bool IsPlaying => player?.IsPlaying ?? false;

    public double Position => player?.CurrentPosition ?? 0;

    public double Duration => player?.Duration ?? 1;

    public void Play(Song song)
    {
        Stop();

        CurrentSong = song;

        var stream = File.OpenRead(song.FilePath);

        player = AudioManager.Current.CreatePlayer(stream);

        player.Play();

        SongChanged?.Invoke();
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
}