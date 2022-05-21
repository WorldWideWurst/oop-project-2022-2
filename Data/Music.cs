using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Data
{

    public enum SourceType 
    {
        Audio,
        AlbumCover,
        Image,
    }


    public class Source : IRecordView
    {
        public string Address { get; }
        public SourceType SourceType { get; set; } = SourceType.Audio;
        public Guid MusicId { get; set; }


        public Source(string address, Guid musicId, SourceType sourceType = SourceType.Audio) { 
            Address = address;
            SourceType = sourceType;
            MusicId = musicId;
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Insert()
        {
            Database.Instance.InsertSource(this);
        }

        public void Save()
        {
            Database.Instance.SaveSource(this);
        }
    }

    public struct MusicByArtist : IRecordView
    {
        public Guid MusicId { get; }
        public string ArtistId { get; }

        public MusicByArtist(Guid musicId, string artistId)
        {
            MusicId = musicId;
            ArtistId = artistId;
        }

        public void Delete() => Database.Instance.DeleteMusicByArtist(this);
        public void Insert() => Database.Instance.InsertMusicByArtist(this);
        public void Save() { }
    }

    public class Music : IRecordView
    {
        public Guid Id { get; }
        public string? Title { get; set; }
        public string? Album { get; set; }

        public IEnumerable<MusicByArtist> Artists => Database.Instance.GetMusicArtists(this);

        public IEnumerable<Source> Sources => Database.Instance.GetMusicSources(this);

        public Music(Guid id, string? title, string? album) {
            Id = id;
            Title = title;
            Album = album;
        }

        public Music(string? title, string? album) : this(Guid.NewGuid(), title, album) { }


        public void Delete() => throw new NotImplementedException();
        public void Insert() => Database.Instance.InsertMusic(this);
        public void Save() => Database.Instance.SaveMusic(this);
    }

    public class Artist : IRecordView
    {
        public string Name { get; }

        public Artist(string name)
        {
            Name = name;
        }

        public void Delete() => throw new NotImplementedException();

        public void Insert() => Database.Instance.InsertArtist(this);

        public void Save() => Database.Instance.SaveArtist(this);
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
        Compilation,
        Undefined,
    }


    public struct MusicInList : IRecordView
    {
        public Guid MusicId { get; }
        public Guid ListId { get; }

        public MusicInList(Guid musicId, Guid listId)
        {
            MusicId = musicId;
            ListId = listId;
        }

        public void Delete() => Database.Instance.DeleteMusicInList(this);

        public void Insert() => Database.Instance.InsertMusicInList(this);

        public void Save() { }
    }

    public class MusicList : IRecordView
    {

        public Guid Id { get; }
        public string Name { get; set; }
        public string? Owner { get; set; }
        public MusicListType Type { get; set; }
        public DateOnly? PublishDate { get; set; }
        public IEnumerable<MusicInList> Entries => Database.Instance.GetMusicInList(this);

        public MusicList(Guid id, string name, string? owner, DateOnly? publishDate, MusicListType type = MusicListType.Undefined)
        {
            Id = id;
            Name = name;
            Owner = owner;
            PublishDate = publishDate;
            Type = type;
        }

        public MusicList(string name) : this(Guid.NewGuid(), name, null, DateOnly.FromDateTime(DateTime.Now)) { }

        public void Delete() => Database.Instance.DeleteMusicList(this);
        public void Insert() => Database.Instance.InsertMusicList(this);
        public void Save() => Database.Instance.SaveMusicList(this);
    }

}
