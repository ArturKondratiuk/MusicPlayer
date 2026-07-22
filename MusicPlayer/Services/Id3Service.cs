using Id3;
using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Services;
public class Id3Service
{
    public Song ReadSong(string filePath)
    {
        Song song = new Song
        {
            FilePath = filePath,
            Title = Path.GetFileNameWithoutExtension(filePath),
            Artist = "Unknown Artist",
            Album = "Unknown Album"
        };

        try
        {
            var mp3 = new Mp3(filePath);

            var tag = mp3.GetTag(Id3TagFamily.Version2X);

            if (tag != null)
            {
                if (!string.IsNullOrWhiteSpace(tag.Title))
                    song.Title = tag.Title;

                if (!string.IsNullOrWhiteSpace(tag.Album))
                    song.Album = tag.Album;

                if (tag.Artists != null)
                {
                    song.Artist = tag.Artists.ToString();
                }
            }
        }
        catch
        {

        }

        return song;
    }
}