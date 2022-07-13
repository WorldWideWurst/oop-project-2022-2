using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// verfasst von Richard Förster

namespace Project.Index
{

    public interface IIndexer
    {
        /// <summary>
        /// Liefert alle (potentiell) abspielbaren Audiodateien in einem Ordner.
        /// </summary>
        /// <param name="path">Der Pfad eines Ordners, der nach abspielbaren Dateien untersucht werden soll.</param>
        /// <returns>Eine Menge von Dateipfaden, welche (potentiell) spielbare Audiodateien enthält.</returns>
        IEnumerable<string> Index(string path);

        /// <summary>
        /// Sammelt alle abspielbaren Dateien mehrerer Ordner zusammen und gibt die in einem "Stream" zurück.
        /// </summary>
        /// <param name="paths">Merhere Pfade zu Ordnern</param>
        /// <returns>Alle (potentiell) abspielbaren Dateien mehrerer Ordner</returns>
        IEnumerable<string> IndexAll(string[] paths)
        {
            IEnumerable<string>? concat = null;
            foreach (var path in paths)
            {
                if (concat == null)
                {
                    concat = Index(path);
                }
                else
                {
                    concat = concat.Concat(Index(path));
                }
            }
            return concat ?? Enumerable.Empty<string>();
        }
    }


    public class SimpleDiskIndexer : IIndexer
    {

        public static readonly SimpleDiskIndexer Instance = new(new string[]
        {
            "mp3",
            "ogg",
            "m4a",
            "wav",
        });

        private string[] playableExtensions;

        public SimpleDiskIndexer(string[] extensions)
        {
            this.playableExtensions = extensions;
        }

        private bool IsPlayable(string filePath)
        {
            foreach (var ext in playableExtensions)
            {
                if (filePath.EndsWith("." + ext))
                {
                    return true;
                }
            }

            return false;
        }


        public IEnumerable<string> Index(string searchPath)
        {
            var fileEnum = Directory.EnumerateFiles(searchPath, "*.*", SearchOption.AllDirectories).GetEnumerator();
            while (true)
            {
                string? file = null;
                try
                {
                    if (!fileEnum.MoveNext()) break;
                    if (IsPlayable(fileEnum.Current))
                        file = fileEnum.Current;
                }
                catch
                {
                    file = null;
                    Console.WriteLine("Fehler bei zugriff in " + searchPath);
                }
                if(file != null) yield return file;

            }
        }


    }

}
