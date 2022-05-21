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
            throw new NotImplementedException();
        }

        internal IEnumerable<Source> GetMusicSources(Music music)
        {
            throw new NotImplementedException();
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
            using var cmd = new SQLiteCommand("insert into source(address, type, _source_of) values (@address, @type, @source_of)", connection); ;
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



        internal void InsertArtist(Artist artist)
        {
            using var cmd = new SQLiteCommand("insert into artist(name) values (@name)", connection);
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
