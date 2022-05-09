using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Database
{
    public interface IMetaLoader
    {
        bool SupportsExtension(string extension);

        Music Load(string path);
    }

    public class MetaLoader : IMetaLoader
    {

        public static readonly MetaLoader Instance = new MetaLoader();

        public IList<IMetaLoader> Loaders { get; } = new List<IMetaLoader>();

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
