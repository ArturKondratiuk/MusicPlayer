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

    public void Play(string filePath)
    {
        Stop();

        var stream = File.OpenRead(filePath);

        player = AudioManager.Current.CreatePlayer(stream);

        player.Play();

        CurrentSong = new Song
        {
            FilePath = filePath
        };

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
}