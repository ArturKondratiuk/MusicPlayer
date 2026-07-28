using Plugin.Maui.Audio;
using MusicPlayer.Models;

namespace MusicPlayer.Services;

public class AudioService
{
    private IAudioPlayer? player;
    private FileStream? currentStream;
    private readonly IDispatcherTimer timer;
    private readonly AlbumArtService albumArtService = new();

    public event Action? PlaybackUpdated;
    public event Action? SongChanged;

    public Song? CurrentSong { get; private set; }

    public List<Song> Playlist { get; private set; } = new();

    public int CurrentIndex { get; private set; } = -1;

    public bool IsPlaying => player?.IsPlaying ?? false;

    public double Position => player?.CurrentPosition ?? 0;

    public double Duration => player?.Duration ?? 0;

    public AudioService()
    {
        timer = Application.Current!.Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(200);

        timer.Tick += (_, _) =>
        {
            if (player != null)
                PlaybackUpdated?.Invoke();
        };

        timer.Start();
    }

    public void SetPlaylist(List<Song> songs)
    {
        Playlist = songs;
    }

    public async Task Play(Song song)
    {
        Stop();

        CurrentSong = song;
        CurrentIndex = Playlist.IndexOf(song);

        currentStream = File.OpenRead(song.FilePath);

        player = AudioManager.Current.CreatePlayer(currentStream);

        player.Play();

        CurrentSong.AlbumArt =
            await albumArtService.GetCoverUrlAsync(
                CurrentSong.Artist,
                CurrentSong.Album);

        SongChanged?.Invoke();
        PlaybackUpdated?.Invoke();
    }

    public void Pause()
    {
        player?.Pause();
        PlaybackUpdated?.Invoke();
    }

    public void Resume()
    {
        player?.Play();
        PlaybackUpdated?.Invoke();
    }

    public void TogglePlayPause()
    {
        if (player == null)
            return;

        if (player.IsPlaying)
            player.Pause();
        else
            player.Play();

        PlaybackUpdated?.Invoke();
    }

    public void Stop()
    {
        player?.Stop();
        player?.Dispose();
        player = null;

        currentStream?.Dispose();
        currentStream = null;
    }

    public void Seek(double position)
    {
        if (player?.CanSeek == true)
        {
            player.Seek(position);
            PlaybackUpdated?.Invoke();
        }
    }

    public async Task Next()
    {
        if (CurrentIndex < Playlist.Count - 1)
            await Play(Playlist[CurrentIndex + 1]);
    }

    public async Task Previous()
    {
        if (CurrentIndex > 0)
            await Play(Playlist[CurrentIndex - 1]);
    }
}