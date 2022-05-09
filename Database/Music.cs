using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Database
{
    public class Music
    {

        public string? Title { get; set; }

        public string? Album { get; set; }

        public IList<string> Interprets { get; } = new List<string>();

        public IList<string> Sources { get; } = new List<string>();

        public DateTime Version { get; set; }

    }

}
