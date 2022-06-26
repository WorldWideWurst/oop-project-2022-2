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
        Local,
        Stream,
    }


    public class Source : IRecordView
    {
        public string Address { get; }
        public SourceType SourceType { get; set; } = SourceType.Local;
        public Guid MusicId { get; set; }
        public ulong? Checksum { get; set; }


        public Source(string address)
        {
            Address = address;
        }

        public void Delete() => throw new NotImplementedException();
        public void Insert() => Database.Instance.InsertSource(this);
        public void Save() => Database.Instance.SaveSource(this);
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

    public enum MusicType
    {
        Undefined,
        Song,
        Audiobook,
        Sound,
        Mixtape,
        BackgroundMusic,
    }

    public class Music : IRecordView
    {
        public Guid Id { get; }
        public string? Title { get; set; }
        public string? Album { get; set; }
        public Guid? AlbumId { get; set; }
        public DateTime? LastPlayed { get; set; }
        public DateTime FirstRegistered { get; set; }
        public string? Art { get; set; }
        public TimeSpan? Duration { get; set; }
        public MusicType Type { get; set; } = MusicType.Undefined;
        public uint PlayCount { get; set; } = 0;

        public IEnumerable<MusicByArtist> Artists => Database.Instance.GetMusicArtists(this);
        public IEnumerable<Source> Sources => Database.Instance.GetMusicSources(this);
        public bool IsCurrentlyPlaying
        {
            get => Player.Player.Instance.CurrentMusic?.Id == Id;
            set
            {
                Player.Player.Instance.PlayImmediately(this);
            }
        }

        public Music(Guid id)
        {
            Id = id;
        }

        public Music()
        {
            Id = Guid.NewGuid();
        }

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

    public class Art : IRecordView
    {
        public string Address { get; }
        public ulong? Checksum { get; set; }

        public Art(string address)
        {
            Address = address;
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Insert()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }

    public enum MusicListType
    {
        Undefined,
        Album,
        ConceptAlbum,
        Single,
        LiveAlbum,
        EP,
        Playlist,
        Queue,
        Compilation,
    }


    public struct MusicInList : IRecordView
    {
        public Guid MusicId { get; }
        public Guid ListId { get; }
        public int? Position { get; set; } = null;
        public DateTime? DateAdded { get; set; } = null;

        public MusicInList(Guid musicId, Guid listId)
        {
            MusicId = musicId;
            ListId = listId;
        }

        public void Delete() => Database.Instance.DeleteMusicInList(this);

        public void Insert() => Database.Instance.InsertMusicInList(this);

        public void Save() { }
    }

    public class MusicList : IRecordView, IMusicList
    {

        public Guid Id { get; }
        public string Name { get; set; }
        public string? Owner { get; set; }
        public MusicListType Type { get; set; } = MusicListType.Undefined;
        public DateTime? PublishDate { get; set; }
        public string? Art { get; set; }
        public bool IsDeletable { get; set; }

        public IEnumerable<MusicInList> Entries => Database.Instance.GetMusicInList(this);
        public IEnumerable<Music> MusicEntries => Database.Instance.GetMusicInListDirect(this);

        public string? CoverArtSource => "/UI/Images/heart_active.png";
        IEnumerable<Music> IMusicList.Entries => MusicEntries;

        public MusicList(Guid id) { Id = id; }

        public MusicList() : this(Guid.NewGuid()) {  }

        public void Delete() => Database.Instance.DeleteMusicList(this);
        public void Insert() => Database.Instance.InsertMusicList(this);
        public void Save() => Database.Instance.SaveMusicList(this);
    }

}
