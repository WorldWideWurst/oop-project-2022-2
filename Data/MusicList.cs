﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Data
{
    public interface IMusicList
    {
        string Name { get; }
        string? CoverArtSource { get; }
        string? OwnerName { get; }
        string? Description { get; }
        IEnumerable<Music> Entries { get; }

        int Count => Entries.Count();
    }

    public class UnregisteredMusicList : IMusicList
    {
        public string Name => "Nicht eingeordnet";
        public string? CoverArtSource => "/UI/Images/heart_hover.png";
        public string? OwnerName => null;
        public string? Description => null;

        public IEnumerable<Music> Entries => Database.Instance.GetMusicWithoutAlbum();
        
    }

    public class AllMusicList : IMusicList
    {
        public string Name => "Alle Lieder";
        public string? CoverArtSource => "/UI/Images/heart_hover.png";
        public string? OwnerName => null;
        public string? Description => null;

        public IEnumerable<Music> Entries => Database.Instance.GetMusic();

    }

    public class FavouritesList : IMusicList
    {
        public string Name => "Lieblingslieder";
        public string? CoverArtSource => "/UI/Images/heart_hover.png";
        public string? OwnerName => null;
        public string? Description => null;

        public IEnumerable<Music> Entries => Database.Instance.GetMusicList(Database.FavouritesPlaylistId).MusicEntries.Target;
    }
}
