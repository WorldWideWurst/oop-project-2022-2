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

        public const string Version = "0.1";
        
        public static readonly string DefaultDBLoc = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\.music_db\\database\\{Version.Replace(".", "_")}.sqlite";
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

        public Music GetMusic(Guid id)
        {
            throw new NotImplementedException();
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
