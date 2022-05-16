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

    public interface IDatabaseRecord
    {
        void WriteToDB();
    }


    public enum SourceType
    {
        Audio,
        AlbumCover,
        Image
    }

    public record struct Source(string source, SourceType sourceType);


    public class Music : IDatabaseRecord
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Album { get; set; }
        public IList<string> Interprets { get; set; }
        public IList<Source> Sources { get; set; }

        private Database database;

        internal Music(Database database, Guid id, string? title, string? album, IList<string> interprets, IList<Source> sources)
        {
            this.database = database;
            this.Id = id;
            this.Title = title;
            this.Album = album;
            this.Interprets = interprets;
            this.Sources = sources;
        }

        public void WriteToDB()
        {
            throw new NotImplementedException();
        }
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

    public class MusicList : MusicObject, IDatabaseRecord
    {

        public IList<Music> Entries { get; set; }

        public string? Owner { get; set; }

        public MusicListType Type { get; set; }

        public DateOnly? PublishDate { get; set; }

        public MusicList(IList<Music> entries, string? owner, DateOnly? publishDate, MusicListType type = MusicListType.Undefined)
        {
            Entries = entries;
            Owner = owner;
            PublishDate = publishDate;
            Type = type;
        }

        public void WriteToDB()
        {
            throw new NotImplementedException();
        }

    }

}
