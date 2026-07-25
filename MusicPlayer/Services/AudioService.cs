using Plugin.Maui.Audio;
using MusicPlayer.Models;

namespace MusicPlayer.Services;

public class AudioService
{
    private IAudioPlayer? player;
    private readonly IDispatcherTimer timer;
    private FileStream? currentStream;

    public event Action? PlaybackUpdated;
    public event Action? SongChanged;

    public Song? CurrentSong { get; private set; }

    public List<Song> Playlist { get; private set; } = new();

    public int CurrentIndex { get; private set; } = -1;

    public bool IsPlaying => player?.IsPlaying ?? false;

    public double Position => player?.CurrentPosition ?? 0;

    public double Duration => player?.Duration ?? 1;

    public string PositionText =>
        TimeSpan.FromSeconds(Position).ToString(@"mm\:ss");

    public string DurationText =>
        TimeSpan.FromSeconds(Duration).ToString(@"mm\:ss");

    public AudioService()
    {
        timer = Application.Current!.Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(200);

        timer.Tick += (_, _) =>
        {
            PlaybackUpdated?.Invoke();
        };

        timer.Start();
    }

    public void SetPlaylist(List<Song> songs)
    {
        Playlist = songs;
    }

    public void Play(Song song)
    {
        Stop();

        CurrentSong = song;
        CurrentIndex = Playlist.IndexOf(song);

        currentStream = File.OpenRead(song.FilePath);

        player = AudioManager.Current.CreatePlayer(currentStream);

        player.Play();

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
        if (player != null)
        {
            player.Stop();
            player.Dispose();
            player = null;
        }

        if (currentStream != null)
        {
            currentStream.Dispose();
            currentStream = null;
        }

        PlaybackUpdated?.Invoke();
    }

    public void Seek(double position)
    {
        if (player == null)
            return;

        if (!player.CanSeek)
            return;

        player.Seek(position);

        PlaybackUpdated?.Invoke();
    }

    public void Next()
    {
        if (Playlist.Count == 0)
            return;

        if (CurrentIndex >= Playlist.Count - 1)
            return;

        Play(Playlist[CurrentIndex + 1]);
    }

    public void Previous()
    {
        if (Playlist.Count == 0)
            return;

        if (CurrentIndex <= 0)
            return;

        Play(Playlist[CurrentIndex - 1]);
    }
}