using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Data
{

    public interface IRecordRelation<T>
    {
        T? Target { get; }
        void ForceReload();
    }

    public class Link<K, T> : IRecordRelation<T>
    {
        public K? Key
        {
            get => key;
            set { key = value; loaded = false; }
        }
        K? key = default;

        public T? Target
        {
            get
            {
                if(!loaded)
                {
                    target = GetTarget(Key);
                    loaded = true;
                }
                return target;
            }
        }
        T? target = default;
        bool loaded = false;

        public Func<K?, T?> GetTarget { get; set; }


        public Link(K? key, Func<K?, T?> targetGetter)
        {
            this.key = key;
            this.GetTarget = targetGetter;
        }

        public void ForceReload() => loaded = false;
    }

    public class Rel<T> : IRecordRelation<T>
    {
        public T? Target
        {
            get
            {
                if(!loaded)
                {
                    target = GetTarget();
                    loaded = true;
                }
                return target;
            }
        }
        T? target = default;
        bool loaded = false;

        public Func<T?> GetTarget { get; set; }

        public Rel(Func<T?> targetGetter)
        {
            GetTarget = targetGetter;
        }

        public void ForceReload() => loaded = false;


    }



    public enum SourceType 
    {
        Local,
        Stream,
    }


    public class Source : IRecordView
    {
        public string Address { get; }
        public SourceType SourceType { get; set; } = SourceType.Local;
        public Guid MusicId
        {
            get => Music.Key;
            set => Music.Key = value;
        }
        public Link<Guid, Music> Music { get; } = new(default, id => Database.Instance.GetMusic(id));
        public ulong? Checksum { get; set; }

        public Source(string address, Guid musicId)
        {
            Address = address;
            MusicId = musicId;
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
        public string? AlbumName { get; set; }
        public Guid? AlbumId 
        {
            get => Album.Key;
            set => Album.Key = value;
        }
        public Link<Guid?, MusicList> Album { get; }
        public DateTime? LastPlayed { get; set; }
        public DateTime FirstRegistered { get; set; }
        public string? ArtAddress
        {
            get => Art.Key;
            set => Art.Key = value;
        }
        public Link<string?, Art?> Art { get; }
        public TimeSpan? Duration { get; set; }
        public MusicType MusicType { get; set; } = MusicType.Undefined;
        public uint PlayCount { get; set; } = 0;

        public Rel<MusicByArtist[]> Artists { get; }

        public Rel<Source[]> Sources { get; }

        public Art? AlbumArt => Album.Target?.Art.Target;
       
        public Music(Guid id)
        {
            Id = id;
            Art = new(default, adr => adr != null ? Database.Instance.GetArt(adr) : null);
            Album = new(default, id => id != null ? Database.Instance.GetMusicList(id.Value) : null);
            Artists = new(() => Database.Instance.GetMusicArtists(this).ToArray());
            Sources = new(() => Database.Instance.GetMusicSources(this).ToArray());
        }

        public Music() : this(Guid.NewGuid()) { }

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
        public string? OwnerName 
        {
            get => Owner.Key;
            set => Owner.Key = value;
        }
        public Link<string?, Artist?> Owner { get; }
        public MusicListType Type { get; set; } = MusicListType.Undefined;
        public DateTime? PublishDate { get; set; }
        public string? ArtAddress 
        {
            get => Art.Key;
            set => Art.Key = value;
        }
        public Link<string?, Art?> Art { get; }
        public bool IsDeletable { get; set; }


        public Rel<MusicInList[]> Entries;
        public Rel<Music[]> MusicEntries;

        public string? CoverArtSource => "/UI/Images/heart_active.png";
        IEnumerable<Music> IMusicList.Entries => MusicEntries.Target;
        string? IMusicList.Description => null;

        public MusicList(Guid id)
        {
            Id = id;
            Owner = new(null, o => o != null ? Database.Instance.GetArtist(o) : null);
            Art = new(null, a => a != null? Database.Instance.GetArt(a) : null);
            Entries = new(() => Database.Instance.GetMusicInList(this).ToArray());
            MusicEntries = new(() => Database.Instance.GetMusicInListDirect(this).ToArray());
        }

        public MusicList() : this(Guid.NewGuid()) {  }

        public void Delete() => Database.Instance.DeleteMusicList(this);
        public void Insert() => Database.Instance.InsertMusicList(this);
        public void Save() => Database.Instance.SaveMusicList(this);
    }

}
