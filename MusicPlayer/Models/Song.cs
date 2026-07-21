using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Models;

public class Song
{
    public string FilePath { get; set; } = "";

    public string Title { get; set; } = "";

    public string Artist { get; set; } = "";

    public string Album { get; set; } = "";

    public TimeSpan Duration { get; set; }

    public string AlbumArtUrl { get; set; } = "";
}
