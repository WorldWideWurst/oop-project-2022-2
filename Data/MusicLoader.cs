using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Data
{
    public interface IMusicLoader
    {
        bool SupportsExtension(string extension);

        Music Load(string path);
    }

    public class MusicLoader : IMusicLoader
    {

        public static readonly MusicLoader Instance = new MusicLoader();

        public IList<IMusicLoader> Loaders { get; } = new List<IMusicLoader>();

        public bool SupportsExtension(string extension)
        {
            return Loaders.Any(loader => loader.SupportsExtension(extension));
        }

        public Music Load(string path)
        {
            throw new NotImplementedException();
        }
    }

    public class UnsupportedMusicFormat : Exception
    {
        // <3
    }

}
