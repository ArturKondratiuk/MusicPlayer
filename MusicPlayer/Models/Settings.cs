namespace MusicPlayer.Models;

public class Settings {
    //app theme
    public bool DarkMode { get; set; }

    //default library sorting
    public string DefaultSort { get; set; } = "Title";

    //ask before deleting songs
    public bool ConfirmDelete { get; set; } = true;

    //download album covers
    public bool DownloadAlbumArt { get; set; } = true;

    //save shuffle state
    public bool RememberShuffle { get; set; } = true;

    //save repeat mode
    public bool RememberRepeat { get; set; } = true;

    //current shuffle state
    public bool ShuffleEnabled { get; set; }

    //current repeat mode
    public int RepeatModeValue { get; set; }
}