using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Xml;
using System.Reflection;

namespace Project.Music
{
    public class Database : IDisposable
    {

        public const string Version = "0.2";
        
        public static readonly string DefaultDBLoc = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\.music_db\\database\\{Version.Replace(".", "_")}.sqlite3";
        public static readonly string EmptyDBSQLLoc = "Database\\empty_musicdb_template.sqlite3.sql";
        
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

                var command = new SQLiteCommand(File.ReadAllText(sqlFile), connection);
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



        public IEnumerable<MusicObject> StringQuery(string query)
        {
            throw new NotImplementedException();
        }

        private string ensureArtistExists(string name)
        {
            using var cmd = new SQLiteCommand("insert or ignore into artist(name) values (@name)", connection);
            cmd.Parameters.AddWithValue("name", name);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            return name;
        }

        private void ensureMusicByArtistExists(Music music, string artist)
        {
            using var cmd = new SQLiteCommand("insert or ignore into _music_by_artist(_music, _artist) values (@music, @artist)", connection);
            cmd.Parameters.AddWithValue("music", music.Id.ToByteArray());
            cmd.Parameters.AddWithValue("artist", artist);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
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

        private void ensureSourceExists(Music music, Source source)
        {
            using var cmd = new SQLiteCommand("insert or ignore into source(address, type, _source_of) values (@address, @type, @source_of)", connection); ;
            cmd.Parameters.AddWithValue("address", source.Address);
            cmd.Parameters.AddWithValue("type", sourceTypeToString(source.SourceType));
            cmd.Parameters.AddWithValue("source_of", music.Id.ToByteArray());
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public IList<Source> GetMusicSources(Guid id)
        {
            using var cmd = new SQLiteCommand("select address, type from source where _source_of = @music", connection);
            cmd.Parameters.AddWithValue("music", id.ToByteArray());
            cmd.Prepare();

            using var reader = cmd.ExecuteReader();

            var sources = new List<Source>();
            while(reader.Read())
            {
                sources.Add(new Source(reader.GetString(0), stringToSourceType(reader.GetString(1))));
            }

            return sources;
        }

        public IList<string> GetMusicArtists(Guid id)
        {
            using var cmd = new SQLiteCommand("select _artist from _music_by_artist where _music = @music", connection);
            cmd.Parameters.AddWithValue("music", id.ToByteArray());
            cmd.Prepare();

            using var reader = cmd.ExecuteReader();

            var artists = new List<string>();
            while (reader.Read())
            {
                artists.Add(reader.GetString(0));
            }

            return artists;
        }

        public string GetMusicAlbum(Guid id)
        {
            return null; // TODO
        }

        public Music? GetMusic(Guid id)
        {
            var artists = GetMusicArtists(id);
            var sources = GetMusicSources(id);
            var album = GetMusicAlbum(id);

            using var cmd = new SQLiteCommand("select title from music where id = @id", connection);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Prepare();

            string? title = (string?)cmd.ExecuteScalar();

            return new Music(id, title, album, artists, sources);
        }

        public void WriteMusic(Music music)
        {
            using SQLiteCommand cmd = new SQLiteCommand(connection);
            if(!music.Exists)
            {
                cmd.CommandText = "insert into music (id, title) values (@id, @title)";
            }
            else
            {
                cmd.CommandText = "update music set title = @title where id = @id";
            }
            
            cmd.Parameters.AddWithValue("id", music.Id.ToByteArray());
            cmd.Parameters.AddWithValue("title", music.Title);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            foreach(var artist in music.Interprets)
            {
                ensureArtistExists(artist);
                ensureMusicByArtistExists(music, artist);
            }

            foreach(var source in music.Sources)
            {
                ensureSourceExists(music, source);
            }

        }

        public MusicList GetMusicList(Guid id)
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            connection.Dispose();
        }
        
    }
}
