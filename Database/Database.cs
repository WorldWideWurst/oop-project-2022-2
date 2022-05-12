using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Xml;

namespace Project.Music
{
    public class Database : IDisposable
    {

        private IDictionary<Guid, Music> music = new Dictionary<Guid, Music>();
        private ISet<string> registeredSources = new HashSet<string>();

        // auto-generierte Listen
        private MusicList favourites = new MusicList();
        private IList<MusicList> playlists = new List<MusicList>();


        public readonly Database Instance = new();

        public Database(string path)
        {
            // Datenbank kreieren falls nicht existent
            if(!File.Exists(path))
            {
                string? parent = Directory.GetParent(path)?.FullName;
                if (!Directory.Exists(parent)) Directory.CreateDirectory(parent);

                File.Copy("Database\\EmptyMusicDBTemplate.xml", path);
            }

            // Daten einlesen
            var doc = new XmlDocument();
                doc.Load(File.Open(path, FileMode.Open));

            foreach (XmlNode item in doc.DocumentElement?.ChildNodes)
            {
                if(item.Name == "music")
                {
                    addMusicFromXml(item);
                }
                else if(item.Name == "playlist")
                {
                    addPlaylistFromXml(item);
                }
            }
        }

        private void addPlaylistFromXml(XmlNode item)
        {
            throw new NotImplementedException();
        }

        private void addMusicFromXml(XmlNode xml)
        {
            var id = xml.Attributes?["id"]?.Name;
            Guid guid = id != null ? Guid.Parse(id) : Guid.NewGuid();

            Music music = new Music { Id = guid };

            foreach(XmlNode item in xml.ChildNodes)
            {
                if(item.Name == "title")
                {
                    music.Title = item.InnerText;
                } 
                else if(item.Name == "album")
                {
                    music.Album = item.InnerText;
                } 
                else if(item.Name == "interpret")
                {
                    music.Interprets.Add(item.InnerText);
                }
                else if(item.Name == "source")
                {
                    music.Sources.Add(item.InnerText);
                    registeredSources.Add(item.InnerText);
                }
            }

            this.music[music.Id] = music;
        }

        public Database() : this("Datastore\\MusicDB.xml") { } // TODO: pfad später ändern

        public void Dispose()
        {
            // Datenbank ausschreiben
            // 
        }

        public Music Register(string source)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MusicObject> StringQuery(string query)
        {
            throw new NotImplementedException();
        }
        
    }
}
