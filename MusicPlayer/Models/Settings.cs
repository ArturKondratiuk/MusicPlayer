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
}
