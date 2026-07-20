using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Models;

namespace MusicPlayer.Services
{
    internal class SettingsService
    {
        public AppSettings Settings { get; } = new();
    }
}
