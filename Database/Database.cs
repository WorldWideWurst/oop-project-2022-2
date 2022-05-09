using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Music
{
    public class Database
    {

        private IList<Music> all = new List<Music>();
        private ISet<string> registeredSources = new HashSet<string>();


        public Database()
        {

        }

        public Music Register(string source)
        {
            throw new NotImplementedException();
        }
    }
}
