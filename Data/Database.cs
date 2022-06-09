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
                string? title = reader[0] as string;
                string? album = reader[1] as string;
                return new Music(id, title, album);
            }

            return null;
        }

        public IEnumerable<Music> GetMusic(string? input = null, Range? range = null)
        {
            using var cmd = new SQLiteCommand(connection);
            if(input == null)
            {
                cmd.CommandText = "select id, title, album from music";
            }
            else
            {
                var titleQuery = "%" + input.Replace(' ', '%') + "%";
                cmd.CommandText = "select id, title, album from music where title like @title";
                cmd.Parameters.AddWithValue("@title", titleQuery);
                cmd.Prepare();
            }
            
            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                yield return new Music(reader.GetGuid(0), reader[1] as string, reader[2] as string);
            }
        }

        public IEnumerable<Music> GetMusicWithoutAlbum()
        {
            using var cmd = new SQLiteCommand(connection);
            cmd.CommandText =
                @"select m.id, m.title, m.album from music m
                left join _music_in_list ml on m.id = ml._music
				where ml._list is null";

            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                yield return new Music(reader.GetGuid(0), reader[1] as string, reader[2] as string);
            }
        }

        internal void InsertMusic(Music music)
        {
            using SQLiteCommand cmd = new("insert into music (id, title, album) values (@id, @title, @album)", connection);
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
                yield return new Source(reader.GetString(0), music.Id, stringToSourceType(reader[1] as string ?? ""));
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
                return new Source(address, reader.GetGuid(1), stringToSourceType(reader[0] as string ?? ""));
            }
            return null;
        }

        public IEnumerable<Source> GetSource(string? query = null, Range? range = null)
        {
            using var cmd = new SQLiteCommand(connection);
            if (query == null)
            {
                cmd.CommandText = "select address, type, _source_of from source";
            }
            else
            {
                var addrQuery = "%" + query.Replace("%", "\\%").Replace("_", "\\_").Replace(" ", "%") + "%";
                cmd.CommandText = "select address, type, _source_of from source where address like @query";
                cmd.Parameters.AddWithValue("@query", addrQuery);
                cmd.Prepare();
            }

            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                var address = reader.GetString(0);
                var type = stringToSourceType(reader[1] as string ?? "");
                var sourceOf = reader.GetGuid(2);
                yield return new Source(address, sourceOf, type);
            }
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
            return (long)cmd.ExecuteScalar() > 0 ? new Artist(name) : null;
        }

        public IEnumerable<Artist> GetArtist(string? query, Range? range)
        {
            using var cmd = new SQLiteCommand(connection);
            if(query == null)
            {
                cmd.CommandText = "select name from artist";
            }
            else
            {
                var nameQuery = "%" + query.Replace(" ", "%") + "%";
                cmd.CommandText = "select name from artist where name = @query";
                cmd.Parameters.AddWithValue("@query", nameQuery);
                cmd.Prepare();

            }

            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                yield return new Artist(reader.GetString(0));
            }
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
            while (reader.Read())
            {
                string name = reader.GetString(0);
                MusicListType type = stringToMusicListType(reader[1] as string ?? "");
                string? publishDateStr = reader[2] as string;
                DateOnly? publishDate = publishDateStr != null ? DateOnly.FromDateTime(DateTime.Parse(publishDateStr)) : null;
                string? owner = reader[3] as string;
                return new MusicList(id, name, owner, publishDate, type);
            }
            return null;
        }

        public IEnumerable<MusicList> GetMusicList(string? query = null)
        {
            using var cmd = new SQLiteCommand(connection);
            if (query == null)
            {
                cmd.CommandText = "select id, name, type, publish_date, _owned_by from music_list";
            }
            else
            {
                query = "%" + query.Replace(" ", "%") + "%";
                cmd.CommandText = "select id, name, type, publish_date, _owned_by from music_list where name like @name";
                cmd.Parameters.AddWithValue("@name", query);
                cmd.Prepare();
            }

            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                Guid id = reader.GetGuid(0);
                string name = reader.GetString(1);
                MusicListType type = stringToMusicListType(reader[2] as string ?? string.Empty);
                string? publishDateStr = reader[3] as string;
                DateOnly? publishDate = publishDateStr != null ? DateOnly.FromDateTime(DateTime.Parse(publishDateStr)) : null;
                string? owner = reader[4] as string;
                yield return new MusicList(id, name, owner, publishDate, type);
            }
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

        internal IEnumerable<Music> GetMusicInListDirect(MusicList musicList)
        {
            using var cmd = new SQLiteCommand(@"
                select m.id, m.title, m.album from _music_in_list ml
                inner join music_list l on l.id = ml._list 
                inner join music m on m.id = ml._music
                where l.id = @id
            ", connection);
            cmd.Parameters.AddWithValue("@id", musicList.Id);
            cmd.Prepare();

            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                Guid id = reader.GetGuid(0);
                string? title = reader[1] as string;
                string? album = reader[2] as string;
                yield return new Music(id, title, album);
            }
        }




        public Music RegisterMusicSource(MusicFileMeta meta)
        {
            // musik-Eintrag hinzufügen
            // TODO: gibt es diese Musik schon?
            Music music = new Music(meta.Title, meta.Album);
            music.Insert();

            // künstler hinzufügen und zu künstlern hinzufügen
            foreach (var artistName in meta.Artists)
            {
                if (GetArtist(artistName) == null)
                    new Artist(artistName).Insert();
                new MusicByArtist(music.Id, artistName).Insert();
            }

            // ins album hinzufügen, falls albumdaten vorhanden
            if (music.Album != null)
            {
                MusicList? album = GetMusicList(music.Album).FirstOrDefault();
                if (album == null)
                {
                    album = new MusicList(music.Album);
                    album.Insert();
                }
                new MusicInList(music.Id, album.Id).Insert();
            }

            // quellenangabe hinzufügen
            if(meta.File != null)
            {
                new Source(meta.File, music.Id).Insert();
            }

            return music;
        }

        public IEnumerable<IRecordView> StringQuery(string query)
        {
            throw new NotImplementedException();
        }

        public void ClearAll()
        {
            string[] tables =
            {
                "_music_by_artist",
                "_music_in_list",
                "source",
                "music_list",
                "artist",
                "music"
            };

            foreach(var table in tables)
            {
                using var cmd = new SQLiteCommand($"delete from {table}", connection);
                cmd.ExecuteNonQuery();
            }
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
