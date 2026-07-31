using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Models;

public class Settings
{
    public bool DarkMode { get; set; }

    public string DefaultSort { get; set; } = "Title";

    public bool ConfirmDelete { get; set; } = true;

    public bool DownloadAlbumArt { get; set; } = true;

    public bool RememberShuffle { get; set; } = true;

    public bool RememberRepeat { get; set; } = true;

    public bool ShuffleEnabled { get; set; }

    public int RepeatModeValue { get; set; }
}