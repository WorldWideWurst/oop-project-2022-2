using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Data.Common;
using System.Data;

// Verfasst von Richard Förster
namespace Project.Data
{

    /// <summary>
    /// Enthält Informationen zu den jeweiligen Datenbank-Tabellen nd wie zu den entsprechenden
    /// IRecordView-Derivat-Properties konvertiert werden kann.
    /// </summary>
    /// <typeparam name="Host">Was für ein IRecordView hier im Fokus steht</typeparam>
    public class ConversionTable<Host>
    {

        /// <summary>
        /// Ein Eintrag in der internen "Tabelle"
        /// </summary>
        /// <typeparam name="Host"></typeparam>
        /// <param name="DbName">Name des Attributs in der Datenbank</param>
        /// <param name="Getter">Holt einen Wert aus der entsprechendem IRecordView-Objekt-Property</param>
        public  record Entry<Host>(string DbName, Func<Host, object?> Getter) { }

        /// <summary>
        /// Name der Tabelle in der Datenbank
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Tabelle
        /// </summary>
        public Entry<Host>[] Table
        {
            get => table;
            set
            {
                table = value;
                Index.Clear();
                foreach(var item in table)
                {
                    Index[item.DbName] = item;
                }
            }
        }
        Entry<Host>[] table;

        /// <summary>
        /// Wie viele primary keys die Tabelle hat. Entweder 1 oder 2 in der Datenbank. 
        /// </summary>
        public int PrimaryKeys { get; set; }

        /// <summary>
        /// Um effizient auf Einträge in der Tabelle zuzugreifen.
        /// </summary>
        public Dictionary<string, Entry<Host>> Index { get; private set; } = new();
    }

    /// <summary>
    /// Datenbank für die Datenverwaltung des Players.
    /// Ist als Singleton implementiert, da Programm sowieso nur eine Datenbank existiert.
    /// </summary>
    public class Database : IDisposable
    {
        
        /// <summary>
        /// Version der aktuellen Datenbank. Über die Zeit hat sich das Datenbank-Schema leicht verändert,
        /// nach jeder Änderung des Datenbankschemas muss einfach nur die Version geändert werden - 
        /// und es wird eine neue Datenbank aus dem SQL-Skript empty_musicdb_template.sqlite3.sql
        /// </summary>
        public const string Version = "0.5.2";

        /// <summary>
        /// Wo alle Programmdaten gespeichert werden.
        /// </summary>
        public static readonly string DataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MusicDB";
        /// <summary>
        /// Der Dateipfad der aktuellen Datenbank.
        /// </summary>
        public static readonly string DefaultDBLoc = $"{DataPath}\\database\\{Version.Replace(".", "_")}.sqlite3";
        /// <summary>
        /// Wo, relativ zu dieser Datei, das Erstellungsskript für eine neue leere Datenbank sich befindet.
        /// </summary>
        public static readonly string EmptyDBSQLLoc = "Data\\empty_musicdb_template.sqlite3.sql";

        /// <summary>
        /// Hardcoded-Guid für die Lieblingslieder-Playlist. Funktioniert irgendwie nicht TODO
        /// </summary>
        public static readonly Guid FavouritesPlaylistId = new Guid("{11f83073-7ff4-bc41-a4ff-e792d073f41f}");

        public static readonly Database Instance = new();

        /// <summary>
        /// Event für interessierte, wenn sich etwas in dieser Datenbank ändert.
        /// </summary>
        public event Action<Type, object?> DatabaseChanged;

        /// <summary>
        /// Verbindungs-Objekt zur Datenbank.
        /// </summary>
        private SQLiteConnection connection;

        /// <summary>
        /// Erstellt eine Datenbank-Instanz mit dem entsprechendem Pfad zu der vielleicht
        /// noch nicht existierenden SQLite3 Datei.
        /// </summary>
        /// <param name="path"></param>
        private Database(string path)
        {
            // Datenbank kreieren falls nicht existent
            if(!File.Exists(path))
            {
                // Zuerst die Directories wenn nötig
                string? parent = Directory.GetParent(path)?.FullName;
                if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

                // zugriff zum empty db Script
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string sqlFile = Path.Combine(assemblyFolder, EmptyDBSQLLoc);

                // Connection aufbauen
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

        /// <summary>
        /// Schnelle Ausgabe der Version von SQLite3.
        /// </summary>
        public string? SQLiteVersion => new SQLiteCommand("SELECT SQLITE_VERSION()", connection)?.ExecuteScalar().ToString();


        /// <summary>
        /// Conversion Tables für jede einzige Tabelle in der Datenbank.
        /// </summary>
        #region ConversionTables

        static ConversionTable<Music> MusicConversionTable = new()
        {
            TableName = "music",
            Table = new ConversionTable<Music>.Entry<Music>[]
            {
                new("id", m => m.Id),
                new("title", m => m.Title),
                new("album", m => m.AlbumName),
                new("_album", m => m.AlbumId),
                new("last_played", m => m.LastPlayed?.ToString("s")),
                new("first_registered", m => m.FirstRegistered.ToString("s")),
                new("_art", m => m.ArtAddress),
                new("duration", m => m.Duration),
                new("type", m => m.MusicType),
                new("play_count", m => m.PlayCount),
            },
            PrimaryKeys = 1,
        };

        static ConversionTable<Source> SourceConversionTable = new()
        {
            TableName = "source",
            Table = new ConversionTable<Source>.Entry<Source>[]
            {
                new("address", s => s.Address),
                new("type", s => s.SourceType),
                new("_source_of", s => s.MusicId),
                new("checksum", s => s.Checksum),
            },
            PrimaryKeys = 1,
        };

        static ConversionTable<MusicList> MusicListConversionTable = new()
        {
            TableName = "music_list",
            Table = new ConversionTable<MusicList>.Entry<MusicList>[]
            {
                new("id", l => l.Id),
                new("name", l => l.Name),
                new("type", l => l.Type),
                new("publish_date", l => l.PublishDate?.ToString("s")),
                new("_owned_by", l => l.OwnerName),
                new("_art", l => l.ArtAddress),
                new("is_deletable", l => l.IsDeletable),
            },
            PrimaryKeys = 1,
        };

        static ConversionTable<Artist> ArtistConversionTable = new()
        {
            TableName = "artist",
            Table = new ConversionTable<Artist>.Entry<Artist>[]
            {
                new("name", a => a.Name),
            },
            PrimaryKeys = 1,
        };

        static ConversionTable<Art> ArtConversionTable = new()
        {
            TableName = "art",
            Table = new ConversionTable<Art>.Entry<Art>[]
            {
                new("address", a => a.Address),
                new("checksum", a => a.Checksum),
            },
            PrimaryKeys = 1,
        };

        static ConversionTable<MusicByArtist> MusicByArtistConversionTable = new()
        {
            TableName = "_music_by_artist",
            Table = new ConversionTable<MusicByArtist>.Entry<MusicByArtist>[]
            {
                new("_music", b => b.MusicId),
                new("_artist", b => b.ArtistId),
            },
            PrimaryKeys = 2,
        };

        static ConversionTable<MusicInList> MusicInListConversionTable = new()
        {
            TableName = "_music_in_list",
            Table= new ConversionTable<MusicInList>.Entry<MusicInList>[]
            {
                new("_music", i => i.MusicId),
                new("_list", i => i.ListId),
                new("position", i => i.Position),
                new("date_added", i => i.DateAdded?.ToString("s")),
            },
            PrimaryKeys = 2,
        };

        /// <summary>
        /// Automatisiertes Hinzufügen von Daten zu cmd.Parameters.
        /// </summary>
        /// <typeparam name="Host"></typeparam>
        /// <param name="ct">der Converstion Table für die Tabelle</param>
        /// <param name="cmd">SQLite3Command</param>
        /// <param name="dbName">name des Attributs in der Datenbank</param>
        /// <param name="host">Das IRecordView Objekt, aus dem Daten gelesen werden.</param>
        static void AddParam<Host>(ConversionTable<Host> ct, SQLiteCommand cmd, string dbName, Host host) 
        {
            cmd.Parameters.AddWithValue("@" + dbName, ct.Index[dbName].Getter(host));
        }


        /// <summary>
        /// Fürgt alle daten in den SQLiteCommand hinein.
        /// </summary>
        /// <typeparam name="Host"></typeparam>
        /// <param name="ct">der Converstion Table für die Tabelle</param>
        /// <param name="cmd">SQLite3Command</param>
        /// <param name="host">Das IRecordView Objekt, aus dem Daten gelesen werden.</param>
        static void AddAllParams<Host>(ConversionTable<Host> ct, SQLiteCommand cmd, Host host)
        {
            foreach (var item in ct.Table)
            {
                cmd.Parameters.AddWithValue("@" + item.DbName, item.Getter(host));
            }
        }


        /// <summary>
        /// Fürgt alle daten in den SQLiteCommand hinein, außer die Primary Keys.
        /// </summary>
        /// <typeparam name="Host"></typeparam>
        /// <param name="ct">der Converstion Table für die Tabelle</param>
        /// <param name="cmd">SQLite3Command</param>
        /// <param name="host">Das IRecordView Objekt, aus dem Daten gelesen werden.</param>
        static void AddNonPrimaryParams<Host>(ConversionTable<Host> ct, SQLiteCommand cmd, Host host)
        {
            foreach(var item in ct.Table.Skip(ct.PrimaryKeys))
            {
                cmd.Parameters.AddWithValue("@" + item.DbName, item.Getter(host));
            }
        }

        /// <summary>
        /// Fürgt alle daten in den SQLiteCommand hinein, nur die Primary Keys.
        /// </summary>
        /// <typeparam name="Host"></typeparam>
        /// <param name="ct">der Converstion Table für die Tabelle</param>
        /// <param name="cmd">SQLite3Command</param>
        /// <param name="host">Das IRecordView Objekt, aus dem Daten gelesen werden.</param>
        static void AddPrimaryParams<Host>(ConversionTable<Host> ct, SQLiteCommand cmd, Host host)
        {
            foreach(var item in ct.Table.Take(ct.PrimaryKeys))
            {
                cmd.Parameters.AddWithValue("@" + item.DbName, item.Getter(host));
            }
        }

        /// <summary>
        /// Generiert eine SQL-Insert-Template für eine Tabellenbeschreibung.
        /// </summary>
        static string GenerateInsertQueryString<Host>(ConversionTable<Host> ct)
        {
            var query = new StringBuilder();
            query.Append($"insert or ignore into {ct.TableName}(");
            query.AppendJoin(", ", ct.Table.Select(item => item.DbName));
            query.Append(") values (");
            query.AppendJoin(", ", ct.Table.Select(item => "@" + item.DbName));
            query.Append(')');
            return query.ToString();
        }

        /// <summary>
        /// Generiert eine SQL-Update-Template für eine gewünschte Tabellenbeschreibung.
        /// </summary>
        static string GenerateUpdateQueryString<Host>(ConversionTable<Host> ct, string selector)
        {
            var query = new StringBuilder();
            query.Append($"update {ct.TableName} set ");
            query.AppendJoin(", ", ct.Table.Select(item => $"{item.DbName} = @{item.DbName}"));
            query.Append(' ').Append(selector);
            return query.ToString();
        }

        #endregion


        /// <summary>
        /// Funktionen, um Daten aus Abfrage-Ergebnissen in IRecordView-Objekte umzuwandeln.
        /// </summary>

        #region RecordView from Parser
        static Music parseMusic(DbDataReader reader)
        {
            return new Music(reader.GetGuid(0))
            {
                Title = reader[1] as string,
                AlbumName = reader[2] as string,
                AlbumId = reader[3] is DBNull ? null : reader.GetGuid(3),
                LastPlayed = reader[4] is DBNull ? null : DateTime.Parse((string)reader[4]),
                FirstRegistered = DateTime.Parse((string)reader[5]),
                ArtAddress = reader[6] as string,
                Duration = reader[7] is DBNull ? null : TimeSpan.FromSeconds(reader.GetDouble(7)),
                MusicType = MusicType.Song,
                PlayCount = reader[9] is DBNull ? 0 : (uint)reader.GetInt32(9),
            };
        }

        static MusicList parseMusicList(DbDataReader reader)
        {
            return new MusicList(reader.GetGuid(0))
            {
                Name = (string)reader[1],
                Type = (MusicListType)(long)reader[2],
                PublishDate = reader[3] is DBNull ? null : DateTime.Parse((string) reader[3]),
                OwnerName = reader[4] as string,
                ArtAddress = reader[5] as string,
                IsDeletable = reader.GetBoolean(6),
            };
        }

        static Source parseSource(DbDataReader reader)
        {
            return new Source(reader.GetString(0), reader.GetGuid(2))
            {
                SourceType = (SourceType)(long)reader[1],
                Checksum = reader[3] is DBNull ? null : (ulong)reader[3],
            };
        }

        static Artist parseArtist(DbDataReader reader)
        {
            return new Artist(reader.GetString(0))
            {

            };
        }

        static Art parseArt(DbDataReader reader)
        {
            return new Art(reader.GetString(0))
            {
                Checksum = reader[1] is DBNull ? null : (ulong)reader.GetInt64(1),
            };
        }

        static MusicInList parseMusicInList(DbDataReader reader)
        {
            return new MusicInList(reader.GetGuid(0), reader.GetGuid(1))
            {
                Position = reader[2] is DBNull ? null : reader.GetInt32(2),
                DateAdded = reader[3] is DBNull ? null : DateTime.Parse((string)reader[3]),
            };
        }

        static MusicByArtist parseMusicByArtist(DbDataReader reader)
        {
            return new MusicByArtist(reader.GetGuid(0), reader.GetString(1))
            {

            };
        }

        /// <summary>
        /// Übersetzt alle Zeilen eines Abfrageergebnisses, jede Zeile mit der entsprechenden Parser-Funktion
        /// </summary>
        static IEnumerable<T> parseAll<T>(DbDataReader reader, Func<DbDataReader, T> parser)
        {
            while(reader.Read())
            {
                yield return parser(reader);
            }
        }

        #endregion


        #region Getters, Queries, Inserters, Savers, Deleters

        /// <summary>
        /// Fragt einen Musik-Eintrag aus der Datenbank ab, die die entsprechende ID hat.
        /// </summary>
        /// <param name="id">Musik-Id</param>
        /// <returns>Music</returns>
        public Music? GetMusic(Guid id)
        {
            // der Ablauf einer Abfrage in C#, nur hier erklärt, da sich das Schema durchzieht
            // Command erstellen
            using var cmd = new SQLiteCommand("select * from music where id = @id", connection);
            // parameterdaten hinzufügen
            cmd.Parameters.AddWithValue("@id", id);
            // PREPARE: nur wenn parameter hinzugefügt wurden
            cmd.Prepare();

            // und das Ergebnis übersetzen
            return parseAll(cmd.ExecuteReader(), parseMusic).FirstOrDefault();
        }

        /// <summary>
        /// Fragt alle Musikeinträge ab, die die Wörter, getrennt durch Leerzeichen,
        /// im Titel enthalten. Ist input null, dann werden einfach alle Musikeinträge abgefragt.
        /// Dass eine gewisse Zahl von Einträgen ausgewählt und übersprungen werden kann,
        /// ist noch nicht implementiert.
        /// </summary>
        public IEnumerable<Music> GetMusic(string? input = null, Range? range = null)
        {
            using var cmd = new SQLiteCommand(connection);
            if(input == null)
            {
                cmd.CommandText = "select * from music";
            }
            else
            {
                var titleQuery = "%" + input.Replace(' ', '%') + "%";
                cmd.CommandText = "select * from music where title like @title";
                cmd.Parameters.AddWithValue("@title", titleQuery);
                cmd.Prepare();
            }
            
            return parseAll(cmd.ExecuteReader(), parseMusic);
        }

        /// <summary>
        /// Abfrage für Musikeinträge ohne Album.
        /// </summary>
        public IEnumerable<Music> GetMusicWithoutAlbum()
        {
            using var cmd = new SQLiteCommand(connection);
            cmd.CommandText =
                @"select m.* from music m
                left join _music_in_list ml on m.id = ml._music
				where ml._list is null";

            return parseAll(cmd.ExecuteReader(), parseMusic);
        }

        /// <summary>
        /// Fügt einen Musikeintrag der Datenbank hinzu 
        /// (oder auch nicht falls einer mit gleicher Id schon existiert)
        /// </summary>
        internal void InsertMusic(Music music)
        {
            using SQLiteCommand cmd = new(GenerateInsertQueryString(MusicConversionTable), connection);
            AddAllParams(MusicConversionTable, cmd, music);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            DatabaseChanged?.Invoke(typeof(Music), music.Id);
        }

        /// <summary>
        /// Überschreibt einen (hoffentlich!) existierenden
        /// Eintrag in der Datenbank mit den hier angegebenen Daten
        /// </summary>
        internal void SaveMusic(Music music)
        {
            using var cmd = new SQLiteCommand(GenerateUpdateQueryString(MusicConversionTable, "where id = @id"), connection);
            AddAllParams(MusicConversionTable, cmd, music);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            DatabaseChanged?.Invoke(typeof(Music), music.Id);
        }

        // der rest der folgenden Funktionen laufen nach dem gleichen Schema wie oben ab:
        // Insert, Get, Save, Delete wenn vorhanden, ...

        internal IEnumerable<MusicByArtist> GetMusicArtists(Music music)
        {
            using var cmd = new SQLiteCommand("select * from _music_by_artist where _music = @_music", connection);
            cmd.Parameters.AddWithValue("@_music", music.Id);
            cmd.Prepare();

            return parseAll(cmd.ExecuteReader(), parseMusicByArtist);
        }

        internal IEnumerable<Source> GetMusicSources(Music music)
        {
            using var cmd = new SQLiteCommand("select * from source where _source_of = @_source_of", connection);
            cmd.Parameters.AddWithValue("@_source_of", music.Id);
            cmd.Prepare();

            return parseAll(cmd.ExecuteReader(), parseSource);
        }



        public Source? GetSource(string address)
        {
            using var cmd = new SQLiteCommand("select * from source where address = @address", connection);
            cmd.Parameters.AddWithValue("@address", address);
            cmd.Prepare();

            return parseAll(cmd.ExecuteReader(), parseSource).FirstOrDefault();
        }

        public IEnumerable<Source> GetSource(string? query = null, Range? range = null)
        {
            using var cmd = new SQLiteCommand(connection);
            if (query == null)
            {
                cmd.CommandText = "select * from source";
            }
            else
            {
                var addrQuery = "%" + query.Replace("%", "\\%").Replace("_", "\\_").Replace(" ", "%") + "%";
                cmd.CommandText = "select * from source where address like @query";
                cmd.Parameters.AddWithValue("@query", addrQuery);
                cmd.Prepare();
            }

            return parseAll(cmd.ExecuteReader(), parseSource);
        }



        internal void InsertSource(Source source)
        {
            using var cmd = new SQLiteCommand(GenerateInsertQueryString(SourceConversionTable), connection); ;
            AddAllParams(SourceConversionTable, cmd, source);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            DatabaseChanged?.Invoke(typeof(Source), source.Address);
        }

        internal void SaveSource(Source source)
        {
            using var cmd = new SQLiteCommand(GenerateUpdateQueryString(SourceConversionTable, "where @address = address"), connection);
            AddAllParams(SourceConversionTable, cmd, source);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            DatabaseChanged?.Invoke(typeof(Source), source.Address);
        }


        public Artist? GetArtist(string name)
        {
            using var cmd = new SQLiteCommand("select * from artist where name = @name", connection);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Prepare();
            return parseAll(cmd.ExecuteReader(), parseArtist).FirstOrDefault();
        }

        public IEnumerable<Artist> GetArtist(string? query = null, Range? range = null)
        {
            using var cmd = new SQLiteCommand(connection);
            if(query == null)
            {
                cmd.CommandText = "select * from artist";
            }
            else
            {
                var nameQuery = "%" + query.Replace(" ", "%") + "%";
                cmd.CommandText = "select * from artist where name = @query";
                cmd.Parameters.AddWithValue("@query", nameQuery);
                cmd.Prepare();

            }

            return parseAll(cmd.ExecuteReader(), parseArtist);
        }

        internal void InsertArtist(Artist artist)
        {
            using var cmd = new SQLiteCommand(GenerateInsertQueryString(ArtistConversionTable), connection);
            AddAllParams(ArtistConversionTable, cmd, artist);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            DatabaseChanged?.Invoke(typeof(Artist), artist.Name);
        }

        internal void SaveArtist(Artist artist)
        {
            DatabaseChanged?.Invoke(typeof(Artist), artist.Name);
        }


        internal void InsertMusicByArtist(MusicByArtist rel)
        {
            using var cmd = new SQLiteCommand(GenerateInsertQueryString(MusicByArtistConversionTable), connection);
            AddAllParams(MusicByArtistConversionTable, cmd, rel);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            DatabaseChanged?.Invoke(typeof(MusicByArtist), (rel.MusicId, rel.ArtistId));
        }

        internal void DeleteMusicByArtist(MusicByArtist rel)
        {
            using var cmd = new SQLiteCommand("delete from _music_by_artist where _music = @_music and _artist = @_artist", connection);
            AddPrimaryParams(MusicByArtistConversionTable, cmd, rel);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            DatabaseChanged?.Invoke(typeof(MusicByArtist), (rel.MusicId, rel.ArtistId));
        }




        public MusicList? GetMusicList(Guid id)
        {
            using var cmd = new SQLiteCommand("select * from music_list where id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id.ToByteArray());
            cmd.Prepare();

            return parseAll(cmd.ExecuteReader(), parseMusicList).FirstOrDefault();
        }

        public IEnumerable<MusicList> GetMusicList(string? query = null)
        {
            using var cmd = new SQLiteCommand(connection);
            if (query == null)
            {
                cmd.CommandText = "select * from music_list";
            }
            else
            {
                query = "%" + query.Replace(" ", "%") + "%";
                cmd.CommandText = "select * from music_list where name like @name";
                cmd.Parameters.AddWithValue("@name", query);
                cmd.Prepare();
            }

            return parseAll(cmd.ExecuteReader(), parseMusicList);
        }

        internal void InsertMusicList(MusicList musicList)
        {
            using var cmd = new SQLiteCommand(GenerateInsertQueryString(MusicListConversionTable), connection);
            AddAllParams(MusicListConversionTable, cmd, musicList);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            DatabaseChanged?.Invoke(typeof(MusicList), musicList.Id);
        }

        internal void SaveMusicList(MusicList musicList)
        {
            using var cmd = new SQLiteCommand(GenerateUpdateQueryString(MusicListConversionTable, "where id = @id"), connection);
            AddAllParams(MusicListConversionTable, cmd, musicList);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            DatabaseChanged?.Invoke(typeof(MusicList), musicList.Id);
        }

        internal void DeleteMusicList(MusicList musicList)
        {
            using var cmd = new SQLiteCommand("delete from music_list where id = @id", connection);
            cmd.Parameters.AddWithValue("@id", musicList.Id);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            DatabaseChanged?.Invoke(typeof(MusicList), musicList.Id);
        }

        public MusicInList? GetMusicInList(Guid listId, Guid musicId)
        {
            using var cmd = new SQLiteCommand("select * from _music_in_list where _list = @_list and _music = @_music", connection);
            cmd.Parameters.AddWithValue("@_list", listId);
            cmd.Parameters.AddWithValue("@_music", musicId);
            cmd.Prepare();

            return parseAll(cmd.ExecuteReader(), parseMusicInList).FirstOrDefault();
        }

        internal IEnumerable<MusicInList> GetMusicInList(MusicList musicList)
        {
            using var cmd = new SQLiteCommand("select * from _music_in_list where _list = @list", connection);
            cmd.Parameters.AddWithValue("@list", musicList.Id);
            cmd.Prepare();

            return parseAll(cmd.ExecuteReader(), parseMusicInList);
        }

        internal void InsertMusicInList(MusicInList musicInList)
        {
            using var cmd = new SQLiteCommand(GenerateInsertQueryString(MusicInListConversionTable), connection);
            AddAllParams(MusicInListConversionTable, cmd, musicInList);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            DatabaseChanged?.Invoke(typeof(MusicInList), (musicInList.MusicId, musicInList.ListId));
        }

        internal void DeleteMusicInList(MusicInList musicInList)
        {
            using var cmd = new SQLiteCommand("delete from _music_in_list where _music = @_music and _list = @_list", connection);
            AddPrimaryParams(MusicInListConversionTable, cmd, musicInList);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            DatabaseChanged?.Invoke(typeof(MusicInList), (musicInList.MusicId, musicInList.ListId));
        }

        /// <summary>
        /// Direkter Zugriff auf die Musik in einer MusicList.
        /// </summary>
        internal IEnumerable<Music> GetMusicInListDirect(MusicList musicList)
        {
            using var cmd = new SQLiteCommand(@"
                select m.* from _music_in_list ml
                inner join music_list l on l.id = ml._list 
                inner join music m on m.id = ml._music
                where l.id = @id
            ", connection);
            cmd.Parameters.AddWithValue("@id", musicList.Id);
            cmd.Prepare();

            return parseAll(cmd.ExecuteReader(), parseMusic);
        }

        internal Art? GetArt(string adr)
        {
            using var cmd = new SQLiteCommand("select * from art where address = @address", connection);
            cmd.Parameters.AddWithValue("@address", adr);
            cmd.Prepare();
            return parseAll(cmd.ExecuteReader(), parseArt).FirstOrDefault();
        }



        /// <summary>
        /// Registriert die Daten, die sich in der MusicFileMeta befinden,
        /// in der Datenbank. Quellen-, Artist-, Art-, MusicByArtist- und ein Music-Eintrag werden
        /// erstellt.
        /// </summary>
        /// <returns>
        /// Der Musikeintrag, zu dem nun lauter Daten eingetragen wurden.
        /// </returns>
        public Music RegisterMusicSource(MusicFileMeta meta)
        {
            // musik-Eintrag hinzufügen
            // TODO: gibt es diese Musik schon?
            Music music = new Music()
            {
                Title = meta.Title,
                AlbumName = meta.Album,
            };
            music.Insert();

            // künstler hinzufügen und zu künstlern hinzufügen
            foreach (var artistName in meta.Artists)
            {
                if (GetArtist(artistName) == null)
                    new Artist(artistName).Insert();
                new MusicByArtist(music.Id, artistName).Insert();
            }

            // ins album hinzufügen, falls albumdaten vorhanden
            if (music.AlbumName != null)
            {
                MusicList? album = GetMusicList(music.AlbumName).FirstOrDefault();
                if (album == null)
                {
                    album = new MusicList()
                    {
                        Name = music.AlbumName,
                    };
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

        // War eigentlich für heruntergeladene DAten gedacht, dass diese auch direkt in die Datenbank
        // geschleust werden. Da es aber zu diesem Schritt hier noch nie gekommen ist, wurde erstmal auskommentiert.
        //public Music RegisterMusicDownload(Project.Download.MusicDownloadInfo download)
        //{
        //    if(download.Thumbnail != null)
        //    {
        //        new Art(download.Thumbnail).Insert();
        //    }

        //    var music = new Music()
        //    {
        //        Title = download.Title,
        //        ArtAddress = download.Thumbnail,
        //        Duration = download.Duration,
        //        FirstRegistered = DateTime.Now,
        //        MusicType = MusicType.Song,  
        //    };
        //    music.Insert();

        //    new Artist(download.Artist).Insert();
        //    new MusicByArtist(music.Id, download.Artist).Insert();

        //    if(download.FileLocation != null)
        //    {
        //        new Source(download.FileLocation, music.Id).Insert();
        //    }
        //    new Source(download.Source, music.Id)
        //    {
        //        SourceType = SourceType.Stream
        //    }.Insert();

        //    return music;
        //}

        /// <summary>
        /// API-Idee: Es wird ein Such-String, so wie bei z.B. Google-Search, eingegeben,
        /// Ergebnisse kommen zurück. Eine Suchfunktion im UI ist noch nicht implementiert.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<IRecordView> StringQuery(string query)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Alle DAten aus allen Tabellen Löschen. Repektiert allerdings die Existenz der Lieblings-Playlist nicht! OMG!
        /// </summary>
        public void ClearAll()
        {
            string[] tables =
            {
                "_music_by_artist",
                "_music_in_list",
                "source",
                "music_list",
                "artist",
                "music",
                "art"
            };

            foreach(var table in tables)
            {
                // unbedingte löscung aller einträge der Tabelle.
                using var cmd = new SQLiteCommand($"delete from {table}", connection);
                cmd.ExecuteNonQuery();
            }

            DatabaseChanged?.Invoke(typeof(Source), null);
            DatabaseChanged?.Invoke(typeof(Music), null);
            DatabaseChanged?.Invoke(typeof(MusicInList), null);
            DatabaseChanged?.Invoke(typeof(MusicList), null);
            DatabaseChanged?.Invoke(typeof(Art), null);
            DatabaseChanged?.Invoke(typeof(Artist), null);
            DatabaseChanged?.Invoke(typeof(MusicByArtist), null);
        }

        #endregion

        public void Dispose()
        {
            connection.Dispose();
        }
        
    }

    /// <summary>
    /// Implementierende Klassen sollen einzelne Einträge in einer Datenbanktabelle darstellen.
    /// Pro Tabelle eine implementierende Klasse.
    /// 
    /// Die Klassen sind RecordViews.cs implementiert.
    /// </summary>
    public interface IRecordView
    {
        /// <summary>
        /// Diesen Eintrag aus der Datenbank löschen. Bitte nicht doppelt löschen!
        /// </summary>
        void Delete();

        /// <summary>
        /// Dieser Eintrag soll in die Taballe hinzugefügt werden.
        /// Ist der Eintrag schon vorhanden, soll NICHTS geschehen.
        /// </summary>
        void Insert();

        /// <summary>
        /// Überschreibt die aktuell vorhandenen Daten zu diesem Eintrag
        /// in der Tabelle mit den in diesem Objekt vorhandenen Daten.
        /// </summary>
        void Save();
    }

}
