using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Xml;
using System.Reflection;

namespace Project.Data
{
    public class Database : IDisposable
    {

        public class Test
        {
        }

        public const string Version = "0.2";
        
        public static readonly string DefaultDBLoc = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\.music_db\\database\\{Version.Replace(".", "_")}.sqlite3";
        public static readonly string EmptyDBSQLLoc = "Data\\empty_musicdb_template.sqlite3.sql";

        internal void DeleteSource(Source source)
        {
            throw new NotImplementedException();
        }

        public static readonly Database Instance = new();


        private SQLiteConnection connection;

        public Database(string path)
        {
            // Datenbank kreieren falls nicht existent
            if(!File.Exists(path))
            {
                string? parent = Directory.GetParent(path)?.FullName;
                if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string sqlFile = Path.Combine(assemblyFolder, EmptyDBSQLLoc);

                connection = new SQLiteConnection($"URI=file:{path}");
                connection.Open();

                using var command = new SQLiteCommand(File.ReadAllText(sqlFile), connection);
                command.ExecuteNonQuery();
            }
            else
            {
                connection = new SQLiteConnection($"URI=file:{path}");
                connection.Open();
            }
            

        }


        public Database() : this(DefaultDBLoc) { }

        public string? SQLiteVersion => new SQLiteCommand("SELECT SQLITE_VERSION()", connection)?.ExecuteScalar().ToString();



        public Music? GetMusic(Guid id)
        {
            using var cmd = new SQLiteCommand("select title, album from music where id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Prepare();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string? title = reader.GetString(0);
                string? album = reader.GetString(1);
                return new Music(id, title, album);
            }

            return null;
        }

        public IEnumerable<Music> QueryMusic(string input, int? limit = null)
        {
            var titleQuery = "%" + input.Replace(' ', '%') + "%";
            using var cmd = new SQLiteCommand("select id, title, album from music where title like @title", connection);
            cmd.Parameters.AddWithValue("@title", titleQuery);
            cmd.Prepare();

            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                yield return new Music(reader.GetGuid(0), reader.GetString(1), reader.GetString(2));
            }
        }

        internal void InsertMusic(Music music)
        {
            using SQLiteCommand cmd = new SQLiteCommand("insert into music (id, title, album) values (@id, @title, @album)", connection);
            cmd.Parameters.AddWithValue("@id", music.Id);
            cmd.Parameters.AddWithValue("@title", music.Title);
            cmd.Parameters.AddWithValue("@album", music.Album);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }


        internal void SaveMusic(Music music)
        {
            using SQLiteCommand cmd = new SQLiteCommand("update music set title = @title, album = @album where id = @id", connection);
            cmd.Parameters.AddWithValue("@id", music.Id.ToByteArray());
            cmd.Parameters.AddWithValue("@title", music.Title);
            cmd.Parameters.AddWithValue("@album", music.Album);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        internal IEnumerable<MusicByArtist> GetMusicArtists(Music music)
        {
            using var cmd = new SQLiteCommand("select _artist from _music_by_artist where _music = @music", connection);
            cmd.Parameters.AddWithValue("@music", music.Id);
            cmd.Prepare();

            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                yield return new MusicByArtist(music.Id, reader.GetString(0));
            }
        }

        internal IEnumerable<Source> GetMusicSources(Music music)
        {
            using var cmd = new SQLiteCommand("select address, type from source where _source_of = @music", connection);
            cmd.Parameters.AddWithValue("@music", music.Id);
            cmd.Prepare();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                yield return new Source(reader.GetString(0), music.Id, stringToSourceType(reader.GetString(1)));
            }
        }



        public Source? GetSource(string address)
        {
            using var cmd = new SQLiteCommand("select type, _source_of from source where address = @address", connection);
            cmd.Parameters.AddWithValue("@address", address);
            cmd.Prepare();
            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                return new Source(address, reader.GetGuid(1), stringToSourceType(reader.GetString(0)));
            }
            return null;
        }

        internal void InsertSource(Source source)
        {
            using var cmd = new SQLiteCommand("insert or ignore into source(address, type, _source_of) values (@address, @type, @source_of)", connection); ;
            cmd.Parameters.AddWithValue("@address", source.Address);
            cmd.Parameters.AddWithValue("@type", sourceTypeToString(source.SourceType));
            cmd.Parameters.AddWithValue("@source_of", source.MusicId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        internal void SaveSource(Source source)
        {
            using var cmd = new SQLiteCommand("update source set type = @type, _source_of = @source_of where address = @address", connection);
            cmd.Parameters.AddWithValue("@address", source.Address);
            cmd.Parameters.AddWithValue("@type", sourceTypeToString(source.SourceType));
            cmd.Parameters.AddWithValue("@source_of", source.MusicId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }


        public Artist? GetArtist(string name)
        {
            using var cmd = new SQLiteCommand("select exists(select 1 from artist where name = @name limit 1)", connection);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Prepare();
            return (bool)cmd.ExecuteScalar() ? new Artist(name) : null;
        }

        internal void InsertArtist(Artist artist)
        {
            using var cmd = new SQLiteCommand("insert or ignore into artist(name) values (@name)", connection);
            cmd.Parameters.AddWithValue("@name", artist.Name);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        internal void SaveArtist(Artist artist)
        {

        }


        internal void InsertMusicByArtist(MusicByArtist rel)
        {
            using var cmd = new SQLiteCommand("insert into _music_by_artist(_music, _artist) values (@music, @artist)", connection);
            cmd.Parameters.AddWithValue("@music", rel.MusicId);
            cmd.Parameters.AddWithValue("@artist", rel.ArtistId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        internal void DeleteMusicByArtist(MusicByArtist rel)
        {
            using var cmd = new SQLiteCommand("delete from _music_by_artist where _music = @music and _artist = @artist", connection);
            cmd.Parameters.AddWithValue("@music", rel.MusicId);
            cmd.Parameters.AddWithValue("@artist", rel.ArtistId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }




        public MusicList? GetMusicList(Guid id)
        {
            using var cmd = new SQLiteCommand("select name, type, publish_date, _owned_by from music_list where id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Prepare(); 
            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                var name = reader.GetString(0);
                var type = stringToMusicListType(reader.GetString(1));
                var publishDate = DateOnly.FromDateTime(reader.GetDateTime(2));
                var owner = reader.GetString(3);
                return new MusicList(id, name, owner, publishDate, type);
            }
            return null;
        }

        internal void InsertMusicList(MusicList musicList)
        {
            using var cmd = new SQLiteCommand("insert into music_list(id, name, type, publish_date, _owned_by) values (@id, @name, @type, @publish_date, @owned_by)", connection);
            cmd.Parameters.AddWithValue("@id", musicList.Id);
            cmd.Parameters.AddWithValue("@name", musicList.Name);
            cmd.Parameters.AddWithValue("@type", musicListTypeToString(musicList.Type));
            cmd.Parameters.AddWithValue("@publish_date", musicList.PublishDate);
            cmd.Parameters.AddWithValue("@owned_by", musicList.Owner);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        internal void SaveMusicList(MusicList musicList)
        {
            using var cmd = new SQLiteCommand("update music_list set name = @name, type = @type, publish_date = @publish_date, _owned_by = @owned_by", connection);
            cmd.Parameters.AddWithValue("@name", musicList.Name);
            cmd.Parameters.AddWithValue("@type", musicListTypeToString(musicList.Type));
            cmd.Parameters.AddWithValue("@publish_date", musicList.PublishDate);
            cmd.Parameters.AddWithValue("@owned_by", musicList.Owner);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        internal void DeleteMusicList(MusicList musicList)
        {
            using var cmd = new SQLiteCommand("delete from music_list where id = @id", connection);
            cmd.Parameters.AddWithValue("@id", musicList.Id);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        internal IEnumerable<MusicInList> GetMusicInList(MusicList musicList)
        {
            using var cmd = new SQLiteCommand("select _music from _music_in_list where _list = @list", connection);
            cmd.Parameters.AddWithValue("@list", musicList.Id);
            cmd.Prepare();

            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                yield return new MusicInList(reader.GetGuid(0), musicList.Id);
            }
        }

        internal void InsertMusicInList(MusicInList musicInList)
        {
            using var cmd = new SQLiteCommand("insert into _music_in_list(_music, _list) values (@music, @list)", connection);
            cmd.Parameters.AddWithValue("music", musicInList.MusicId);
            cmd.Parameters.AddWithValue("list", musicInList.ListId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        internal void DeleteMusicInList(MusicInList musicInList)
        {
            using var cmd = new SQLiteCommand("delete from _music_in_list where _music = @music and _list = @list", connection);
            cmd.Parameters.AddWithValue("music", musicInList.MusicId);
            cmd.Parameters.AddWithValue("list", musicInList.ListId);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }






        public Music RegisterSource(string address, bool forceReload = false)
        {
            var source = GetSource(address);
            if (source != null)
            {
                return GetMusic(source.MusicId) ?? throw new System.Data.ConstraintException();
            }

            MusicFileMeta meta = MetaLoader.Instance.Load(address);
            
            Music music = new Music(meta.Title, meta.Album);
            music.Insert();

            foreach(var artistName in meta.Artists)
            {
                if (GetArtist(artistName) == null)
                    new Artist(artistName).Insert();
            }

            new Source(address, music.Id).Insert();

            return music;
        }

        public IEnumerable<object> StringQuery(string query)
        {
            throw new NotImplementedException();
        }


        private static SourceType stringToSourceType(string value)
        {
            switch(value)
            {
                case "audio": return SourceType.Audio;
                case "album_cover": return SourceType.AlbumCover;
                case "image": return SourceType.Image;
                default: throw new ArgumentException(value);
            }
        }

        private static string sourceTypeToString(SourceType sourceType)
        {
            switch (sourceType)
            {
                case SourceType.Audio: return "audio";
                case SourceType.AlbumCover: return "album_cover";
                case SourceType.Image: return "image";
                default: throw new ArgumentException(sourceType.ToString());
            }
        }

        private static MusicListType stringToMusicListType(string? value)
        {
            switch(value)
            {
                case "album": return MusicListType.Album;
                case "concept_album": return MusicListType.ConceptAlbum;
                case "single": return MusicListType.Single;
                case "live_album": return MusicListType.LiveAlbum;
                case "ep": return MusicListType.EP;
                case "playlist": return MusicListType.Playlist;
                case "queue": return MusicListType.Queue;
                case "compilation": return MusicListType.Compilation;
                case "undefined": return MusicListType.Undefined;
                default: return MusicListType.Undefined;
            }
        }

        private static string musicListTypeToString(MusicListType value)
        {
            switch (value)
            {
                case MusicListType.Album: return "album";
                case MusicListType.ConceptAlbum: return "concept_album";
                case MusicListType.Single: return "single";
                case MusicListType.LiveAlbum: return "live_album";
                case MusicListType.EP: return "ep";
                case MusicListType.Playlist: return "playlist";
                case MusicListType.Queue: return "queue";
                case MusicListType.Undefined: return "undefined";
                case MusicListType.Compilation: return "compilation";
                default: return "undefined";
            }
        }

        public void Dispose()
        {
            connection.Dispose();
        }
        
    }

    public interface IRecordView
    {
        void Delete();
        void Insert();
        void Save();
    }


}
