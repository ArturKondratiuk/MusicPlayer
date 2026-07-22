using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Maui.Audio;

namespace MusicPlayer.Services;

public class AudioService
{
    private IAudioPlayer? player;

    public bool IsPlaying => player?.IsPlaying ?? false;

    public void Play(string filePath)
    {
        Stop();

        var stream = File.OpenRead(filePath);

        player = AudioManager.Current.CreatePlayer(stream);

        player.Play();
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