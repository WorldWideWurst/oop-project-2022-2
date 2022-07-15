using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// verfasst von Richard Förster

namespace Project.Data
{

    /// <summary>
    /// Interface für Objekte, die eine Beziehung zwischen Tabellen abstrahieren.
    /// Was auch immer abstrahiert wird, ist über .Target zu erreichen.
    /// </summary>
    /// <typeparam name="T">Der Typ der Relation. Typischerweise ein anderes IRecordView oder ein Array</typeparam>
    public interface IRecordRelation<T>
    {
        /// <summary>
        /// Wert auslesen und dabei zwischenspeichern, um nicht ständig die Relation neu auszuwerten.
        /// </summary>
        T? Target { get; }

        /// <summary>
        /// Erzwingt das neuladen der Relation beim nächsten Aufruf. 
        /// .Target liefert dadurch andere Werte, wenn sich die Datenbank zwischendurch geändert hat.
        /// </summary>
        void ForceReload();
    }

    /// <summary>
    /// Stellt eine Relation zwischen einem Attribut einer Tabelle und zu der Tabelle, zu der verwiesen wird,
    /// her.
    /// </summary>
    /// <example>
    /// Z.B. verweist eine Quelle auf den Musik-Eintrag, zu dem er Gehört. Da wird eine interne GUID per
    /// Anfrage an die Datenbank in ein Music-Objekt erweitert. Manchmal ist aber nicht das gesamte Musik-Objekt
    /// nötig - man kann den SChlüssel (die ID) auch allein auslesen.
    /// </example>
    /// <typeparam name="K">Schlüsseltyp</typeparam>
    /// <typeparam name="T">Target-Typ</typeparam>
    public class Link<K, T> : IRecordRelation<T>
    {
        /// <summary>
        /// Property, um auf den Schlüssel zuzugreifen. Zwischengespeicherter Target-Wert wird auch zurückgesetzt,
        /// wenn der Schlüssel geändert wird.
        /// </summary>
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

        /// <summary>
        /// Eine Funktion, die den Zielwert aus dem Schlüssen ermittelt (via Anfrage an die Datenbank)
        /// </summary>
        public Func<K?, T?> GetTarget { get; set; }


        /// <summary>
        /// Konstruiert einen Link aus einem Initialschlüssel und einem Getter um den Target-Wert auswerten zu können.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="targetGetter"></param>
        public Link(K? key, Func<K?, T?> targetGetter)
        {
            this.key = key;
            this.GetTarget = targetGetter;
        }

        public void ForceReload() => loaded = false;
    }

    /// <summary>
    /// Stellt eine Relation zwischen zwei Tabellen dar, welche nicht direkt an ein Attribut 
    /// der einen Tabelle gebunden ist. Es wird kein Schlüssel abgespeichert.
    /// </summary>
    /// <example>
    /// Von einer Musikliste möchte man vielleicht wissen, welche Musik sie enthält. Das Ergebnis ist Music[],
    /// doch ist dazu in der music_list Tabelle kein Schlüssel dafür abgespeichert. Deswegen nimmt man dafür eine
    /// Rel<Music[]>, keinen Link.
    /// </example>
    /// <typeparam name="T">Zieltyp</typeparam>
    public class Rel<T> : IRecordRelation<T>
    {

        /// <see cref="IRecordRelation{T}"/>
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

        /// <summary>
        /// Ein Getter, der ohne eine Referenz zu einem Schlüssel den Zielwert liefert.
        /// </summary>
        public Func<T?> GetTarget { get; set; }

        /// <summary>
        /// Konstruktor, der eine Rel aus einem Getter für den Zielwert bildet.
        /// </summary>
        /// <param name="targetGetter"></param>
        public Rel(Func<T?> targetGetter)
        {
            GetTarget = targetGetter;
        }

        public void ForceReload() => loaded = false;


    }


    /// <summary>
    /// Gibt an, ob eine Quellenangabe eine Referenz auf eine lokale Datei oder ein Internet-Link ist.
    /// </summary>
    public enum SourceType 
    {
        Local,
        Stream,
    }

    /// <summary>
    /// Eine Quellenangabe für ein Musikstück. Ein Musikstück kann mehrere Quellen haben, aber eine Quelle
    /// gehört nur zu einem Musikstück.
    /// source
    /// </summary>
    public class Source : IRecordView
    {
        /// <summary>
        /// Primary Key, der Dateipfad zu dieser Datei oder eine URL.
        /// </summary>
        public string Address { get; }
        /// <summary>
        /// Typangabe für diese Quelle
        /// </summary>
        /// <see cref="SourceType"/>
        public SourceType SourceType { get; set; } = SourceType.Local;
        /// <summary>
        /// Liefert die Id der zugehörigen Musik.
        /// </summary>
        public Guid MusicId
        {
            get => Music.Key;
            set => Music.Key = value;
        }
        /// <summary>
        /// Link-Objekt für die zugehörige Musik.
        /// </summary>
        public Link<Guid, Music> Music { get; } = new(default, id => Database.Instance.GetMusic(id));
        /// <summary>
        /// Optionale checksum für diese Quelle. Soll zum wiedererkennen dieser Quelle dienen,
        /// falls diese einmal unerwartet verschoben wird.
        /// </summary>
        public ulong? Checksum { get; set; }

        /// <summary>
        /// Erstellt eine Quellenangabe.
        /// Es muss mindestens der Quellpfad und die zugehörige Musik bekannt sein.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="musicId"></param>
        public Source(string address, Guid musicId)
        {
            Address = address;
            MusicId = musicId;
        }

        public void Delete() => throw new NotImplementedException();
        public void Insert() => Database.Instance.InsertSource(this);
        public void Save() => Database.Instance.SaveSource(this);
    }

    /// <summary>
    /// Struktur, um zu verwalten, welche Musik von welchen Künstlern geschaffen wurde.
    /// Es ist eine extra Tabelle, da es sich um eine m-zu-n-Beziehung handelt.
    /// Tabellenname: _music_by_artist
    /// </summary>
    public struct MusicByArtist : IRecordView
    {
        /// <summary>
        /// Guid der Musik.
        /// </summary>
        public Guid MusicId { get; }
        /// <summary>
        /// Name des Künstlers.
        /// </summary>
        public string ArtistId { get; }

        /// <summary>
        /// Neuen MusicByArtist-eintrag erstellen.
        /// </summary>
        public MusicByArtist(Guid musicId, string artistId)
        {
            MusicId = musicId;
            ArtistId = artistId;
        }

        public void Delete() => Database.Instance.DeleteMusicByArtist(this);
        public void Insert() => Database.Instance.InsertMusicByArtist(this);
        public void Save() { }
    }

    /// <summary>
    /// Gibt an, von welchem Typ ein Stück Musik ist. Ist standartmäßig Undefined (und wird auch momentan
    /// nirgends geändert).
    /// </summary>
    public enum MusicType
    {
        Undefined,
        Song,
        Audiobook,
        Sound,
        Mixtape,
        BackgroundMusic,
    }

    /// <summary>
    /// Identifiziert ein Stück Musik. Musik ist hat verschiedene eigene Daten wie Titel, Id, Abspieldaten,
    /// wann hinzugefügt, kann mehrere Quellen und mehrere Künstler haben und kann in mehreren Musiklisten 
    /// vorhanden sein.
    /// Tabellenname: music
    /// </summary>
    public class Music : IRecordView
    {
        /// <summary>
        /// Identifizierende Guid (da Lieder gleichen Namen und gleiches Album haben können - das wäre zu viel)
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// Titel des Musikstücks. Optional, da Angaben fehlen können.
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// Albumname, wie gelesen (oder auch nicht) aus Metadaten.
        /// </summary>
        public string? AlbumName { get; set; }
        /// <summary>
        /// Zugriffs-Property für Album.
        /// "Album" soll ein echter, harter Verweis auf das originale Album (Id eine Musikliste) in der Datenbank sein.
        /// Brauchbar, um zu genau diesem genau zu verweisen (es gibt viele Alben mit gleichen Namen).
        /// </summary>
        public Guid? AlbumId 
        {
            get => Album.Key;
            set => Album.Key = value;
        }
        /// <summary>
        /// Link zur entsprechenden Musikliste.
        /// </summary>
        public Link<Guid?, MusicList> Album { get; }
        /// <summary>
        /// Wann diese Musik zuletzt gespielt wurde.
        /// </summary>
        public DateTime? LastPlayed { get; set; }
        /// <summary>
        /// Wann diese Musik zuerst registriert wurde.
        /// </summary>
        public DateTime FirstRegistered { get; set; }
        /// <summary>
        /// Zugriffs-Property für Art.
        /// </summary>
        public string? ArtAddress
        {
            get => Art.Key;
            set => Art.Key = value;
        }
        /// <summary>
        /// Link zu einem optionalen Art-Objekt, einem musikspezifischen Coverart. Wird in den meisten Fällen
        /// das Albumcover sein.
        /// </summary>
        public Link<string?, Art?> Art { get; }
        /// <summary>
        /// Wie lang diese Musik spielt.
        /// </summary>
        public TimeSpan? Duration { get; set; }
        /// <summary>
        /// Der Typ dieser Musik.
        /// </summary>
        /// <see cref="MusicType"/>
        public MusicType MusicType { get; set; } = MusicType.Undefined;
        /// <summary>
        /// Wie oft diese Musik schon gespielt wurde.
        /// </summary>
        public uint PlayCount { get; set; } = 0;

        /// <summary>
        /// Rel, die die Künstelr als MusicByArtist-Array angibt.
        /// </summary>
        public Rel<MusicByArtist[]> Artists { get; }

        /// <summary>
        /// Rel, die die Quellen zusammenfasst.
        /// </summary>
        public Rel<Source[]> Sources { get; }

        /// <summary>
        /// Direkter Verweis auf Album-Coverart.
        /// </summary>
        public Art? AlbumArt => Album.Target?.Art.Target;
       
        /// <summary>
        /// Musik-Objekt konstruieren aus einer gegebenen Guid.
        /// </summary>
        public Music(Guid id)
        {
            Id = id;
            Art = new(default, adr => adr != null ? Database.Instance.GetArt(adr) : null);
            Album = new(default, id => id != null ? Database.Instance.GetMusicList(id.Value) : null);
            Artists = new(() => Database.Instance.GetMusicArtists(this).ToArray());
            Sources = new(() => Database.Instance.GetMusicSources(this).ToArray());
        }

        /// <summary>
        /// Musik-Objekt konstruieren mit einer neuen, bisher nie vergebenen Guid.
        /// </summary>
        public Music() : this(Guid.NewGuid()) { }

        public void Delete() => throw new NotImplementedException();
        public void Insert() => Database.Instance.InsertMusic(this);
        public void Save() => Database.Instance.SaveMusic(this);
    }

    /// <summary>
    /// Künstlerangabe. Tabellenname: artist
    /// </summary>
    public class Artist : IRecordView
    {
        /// <summary>
        /// Der Name des Künstlers.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Neuen Künstler erstellen, nur aus seinem Namen.
        /// </summary>
        public Artist(string name)
        {
            Name = name;
        }

        public void Delete() => throw new NotImplementedException();
        public void Insert() => Database.Instance.InsertArtist(this);
        public void Save() => Database.Instance.SaveArtist(this);
    }

    /// <summary>
    /// "Kunst", also ein Medium, wie z.B. einem GIF oder ein Bild, was ein Musikstück visuell unterstützt.
    /// Tabellenname: art
    /// </summary>
    public class Art : IRecordView
    {
        /// <summary>
        /// Dateipfad einer Coverart-Datei.
        /// </summary>
        public string Address { get; }
        /// <summary>
        /// Checksum, gleiches wie bei Source.
        /// </summary>
        /// <see cref="Source"/>
        public ulong? Checksum { get; set; }

        /// <summary>
        /// Art-Objekt konstruieren.
        /// </summary>
        public Art(string address)
        {
            Address = address;
        }

        public void Delete() => throw new NotImplementedException();
        public void Insert() => throw new NotImplementedException();
        public void Save() => throw new NotImplementedException();
    }

    /// <summary>
    /// Verschiedene Typen von Musik
    /// </summary>
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
