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

    public enum SourceType 
    {
        Audio,
        AlbumCover,
        Image,
    }

    public record struct Source(string Address, SourceType SourceType = SourceType.Audio);


    public class Music
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Album { get; set; }
        public IList<string> Interprets { get; set; }
        public IList<Source> Sources { get; set; }

        public readonly bool Exists;

        internal Music(
            Guid? id, 
            string? title,
            string? album,
            IList<string> interprets,
            IList<Source> sources)
        {
            Id = id ?? Guid.NewGuid();
            Title = title;
            Album = album;
            Interprets = interprets;
            Sources = sources;
            Exists = id != null;
        }

        internal Music() : this(Guid.NewGuid(), null, null, new List<string>(), new List<Source>()) { }
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

        public Guid Id { get; set; }

        public IList<Music> Entries { get; set; }

        public string? Owner { get; set; }

        public MusicListType Type { get; set; }

        public DateOnly? PublishDate { get; set; }

        public MusicList(Guid id, IList<Music> entries, string? owner, DateOnly? publishDate, MusicListType type = MusicListType.Undefined)
        {
            Id = id;
            Entries = entries;
            Owner = owner;
            PublishDate = publishDate;
            Type = type;
        }

        public MusicList() : this(Guid.NewGuid(), new List<Music>(), null, DateOnly.FromDateTime(DateTime.Now)) { }

    }

}
