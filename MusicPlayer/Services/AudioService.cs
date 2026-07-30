using MusicPlayer.Models;
using Plugin.Maui.Audio;

namespace MusicPlayer.Services;

public class AudioService
{
    private IAudioPlayer? player;
    private FileStream? currentStream;

    private readonly IDispatcherTimer timer;
    private readonly AlbumArtService albumArtService = new();
    private readonly LibraryService libraryService = new();

    public event Action? PlaybackUpdated;
    public event Action? SongChanged;

    public Song? CurrentSong { get; private set; }

    public List<Song> Playlist { get; private set; } = new();

    public int CurrentIndex { get; private set; } = -1;

    public bool Shuffle { get; set; }

    public int RepeatMode { get; set; }

    public bool IsPlaying => player?.IsPlaying ?? false;

    public double Position => player?.CurrentPosition ?? 0;

    public double Duration => player?.Duration ?? 0;

    public AudioService()
    {
        timer = Application.Current!.Dispatcher.CreateTimer();

        timer.Interval = TimeSpan.FromMilliseconds(200);

        timer.Tick += Timer_Tick;

        timer.Start();
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        if (player == null)
            return;

        PlaybackUpdated?.Invoke();

        if (player.IsPlaying)
            return;

        if (Duration > 0 && Position >= Duration - 0.5)
            await SongFinished();
    }

    public void SetPlaylist(List<Song> songs)
    {
        Playlist = songs;
    }

    public async Task Play(Song song)
    {
        StopInternal();

        CurrentSong = song;
        CurrentIndex = Playlist.IndexOf(song);

        currentStream = File.OpenRead(song.FilePath);

        player = AudioManager.Current.CreatePlayer(currentStream);

        player.Volume = 1.0;

        player.Play();

        if (string.IsNullOrWhiteSpace(song.AlbumArt))
        {
            song.AlbumArt = await albumArtService.GetCoverUrlAsync(
                song.Artist,
                song.Album,
                song.Title);

            await libraryService.SaveLibraryAsync(Playlist);
        }

        SongChanged?.Invoke();
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

    public void Stop()
    {
        StopInternal();

        CurrentSong = null;
        CurrentIndex = -1;

        SongChanged?.Invoke();
        PlaybackUpdated?.Invoke();
    }

    private void StopInternal()
    {
        player?.Stop();
        player?.Dispose();
        player = null;

        currentStream?.Dispose();
        currentStream = null;
    }

    public void Seek(double seconds)
    {
        if (player?.CanSeek == true)
        {
            player.Seek(seconds);
            PlaybackUpdated?.Invoke();
        }
    }

    public void SetVolume(double volume)
    {
        if (player == null)
            return;

        player.Pause();
        player.Volume = volume;
        player.Play();
    }

    public double GetVolume()
    {
        return 1.0;
    }

    public async Task Next()
    {
        if (Playlist.Count == 0)
            return;

        if (Shuffle)
        {
            Random rnd = new();

            int next = rnd.Next(Playlist.Count);

            await Play(Playlist[next]);
            return;
        }

        if (CurrentIndex < Playlist.Count - 1)
        {
            await Play(Playlist[CurrentIndex + 1]);
            return;
        }

        if (RepeatMode == 1)
        {
            await Play(Playlist[0]);
        }
    }

    public async Task Previous()
    {
        if (Playlist.Count == 0)
            return;

        if (CurrentIndex > 0)
        {
            await Play(Playlist[CurrentIndex - 1]);
            return;
        }

        if (RepeatMode == 1)
        {
            await Play(Playlist[^1]);
        }
    }

    private async Task SongFinished()
    {
        switch (RepeatMode)
        {
            case 2:
                if (CurrentSong != null)
                    await Play(CurrentSong);
                break;

            default:
                await Next();
                break;
        }
    }
}