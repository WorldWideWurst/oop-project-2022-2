using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Music
{   

    public class MusicObject
    {

    }

    public class Music : MusicObject
    {

        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? Album { get; set; }

        public IList<string> Interprets { get; } = new List<string>();

        public ISet<string> Sources { get; } = new HashSet<string>();

        public DateTime Version { get; set; } = DateTime.MinValue;

        public bool IsFavourite { get; set; }

    }


    public enum MusicListType
    {
        Album,
        ConceptAlbum,
        Single,
        LiveAlbum,
        EP,
        Playlist,
        Queue,
        Undefined,
    }

    public class MusicList : MusicObject
    {

        public IList<Music> Entries { get; set; } = new List<Music>();

        public MusicListType Type { get; set; } = MusicListType.Undefined;

        public string? Owner { get; set; }

        public DateOnly? PublishDate { get; set; }

    }

}
