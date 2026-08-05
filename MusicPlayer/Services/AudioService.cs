using MusicPlayer.Models;
using Plugin.Maui.Audio;

namespace MusicPlayer.Services;

public class AudioService {
    private IAudioPlayer? player;
    private FileStream? currentStream;

    //timer updates player UI
    private readonly IDispatcherTimer timer;

    //services
    private readonly AlbumArtService albumArtService = new();
    private readonly LibraryService libraryService = new();

    //events for UI
    public event Action? PlaybackUpdated;
    public event Action? SongChanged;

    //current song
    public Song? CurrentSong { get; private set; }

    //current playlist
    public List<Song> Playlist { get; private set; } = new();

    public int CurrentIndex { get; private set; } = -1;

    //playback settings
    public bool Shuffle { get; set; }

    public int RepeatMode { get; set; }

    //player state
    public bool IsPlaying => player?.IsPlaying ?? false;

    public double Position => player?.CurrentPosition ?? 0;

    public double Duration => player?.Duration ?? 0;

    private double volume = 1.0;

    public AudioService() {
        timer = Application.Current!.Dispatcher.CreateTimer();

        timer.Interval = TimeSpan.FromMilliseconds(200);

        timer.Tick += Timer_Tick;

        timer.Start();
    }

    //updates playback information
    private async void Timer_Tick(object? sender, EventArgs e) {
        if (player == null)
            return;

        PlaybackUpdated?.Invoke();

        if (player.IsPlaying)
            return;

        if (Duration > 0 && Position >= Duration - 0.5)
            await SongFinished();
    }

    //sets active playlist
    public void SetPlaylist(List<Song> songs) {
        Playlist = songs;
    }

    //plays selected song
    public async Task Play(Song song) {
        StopInternal();

        CurrentSong = song;
        CurrentIndex = Playlist.IndexOf(song);

        currentStream = File.OpenRead(song.FilePath);

        player = AudioManager.Current.CreatePlayer(currentStream);

        player.Volume = volume;

        player.Play();

        //download cover if needed
        var settings = await new SettingsService().LoadAsync();

        if (settings.DownloadAlbumArt && string.IsNullOrWhiteSpace(song.AlbumArt)) {
            song.AlbumArt = await albumArtService.GetCoverUrlAsync(song.Artist, song.Album, song.Title);

            await libraryService.SaveLibraryAsync(Playlist);
        }

        SongChanged?.Invoke();
        PlaybackUpdated?.Invoke();
    }

    //pause or resume playback
    public void TogglePlayPause() {
        if (player == null)
            return;

        if (player.IsPlaying)
            player.Pause();
        else
            player.Play();

        PlaybackUpdated?.Invoke();
    }

    public void Pause() {
        player?.Pause();

        PlaybackUpdated?.Invoke();
    }

    public void Resume() {
        player?.Play();

        PlaybackUpdated?.Invoke();
    }

    //stops playback
    public void Stop() {
        StopInternal();

        CurrentSong = null;
        CurrentIndex = -1;

        SongChanged?.Invoke();
        PlaybackUpdated?.Invoke();
    }

    //releases player resources
    private void StopInternal() {
        player?.Stop();
        player?.Dispose();
        player = null;

        currentStream?.Dispose();
        currentStream = null;
    }

    //changes playback position
    public void Seek(double seconds) {
        if (player?.CanSeek != true)
            return;

        player.Seek(seconds);

        PlaybackUpdated?.Invoke();
    }

    //sets player volume
    public void SetVolume(double value) {
        volume = value;

        if (player != null)
            player.Volume = value;
    }

    public double GetVolume() {
        return volume;
    }

    //plays next song
    public async Task Next() {
        if (Playlist.Count == 0)
            return;

        if (Shuffle) {
            Random random = new();

            int index = random.Next(Playlist.Count);

            await Play(Playlist[index]);

            return;
        }

        if (CurrentIndex < Playlist.Count - 1) {
            await Play(Playlist[CurrentIndex + 1]);

            return;
        }

        if (RepeatMode == 1) {
            await Play(Playlist[0]);
        }
    }

    //plays previous song
    public async Task Previous() {
        if (Playlist.Count == 0)
            return;

        if (CurrentIndex > 0) {
            await Play(Playlist[CurrentIndex - 1]);

            return;
        }

        if (RepeatMode == 1) 
            await Play(Playlist[^1]);
    }

    //called when current song ends
    private async Task SongFinished() {
        if (RepeatMode == 2) {
            if (CurrentSong != null)
                await Play(CurrentSong);

            return;
        }
        await Next();
    }
}