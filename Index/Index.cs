using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Project.Index
{

    public interface IIndexer
    {
        IEnumerable<string> Index(string path);

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

        public static readonly SimpleDiskIndexer Instance = new SimpleDiskIndexer();

        private string[] playableExtensions = new string[]
        {
            "mp3",
            "ogg",
            "m4a",
            "wav",
        };

        public SimpleDiskIndexer() { }

        public SimpleDiskIndexer(string[] playableExtensions)
        {
            this.playableExtensions = playableExtensions;
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
            var outputArray = new List<string>();
            var fileEnum = Directory.EnumerateFiles(searchPath, "*.*", SearchOption.AllDirectories).GetEnumerator();
            while (true)
            {
                try
                {
                    if (!fileEnum.MoveNext()) break;
                    if (IsPlayable(fileEnum.Current))
                        outputArray.Add(fileEnum.Current);
                }
                catch
                {
                    Console.WriteLine("Fehler bei zugriff in " + searchPath);
                }
            }
            return outputArray.ToArray();
        }


    }

}
