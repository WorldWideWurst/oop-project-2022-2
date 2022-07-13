using Project.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// verfasst von Richard Förster

namespace Project.Player
{
    public class Library
    {
        public static readonly Library Instance = new();


        public readonly ObservableCollection<IMusicList> Entries = new();

        public Library() 
        {
            Refresh();
        }

        public void Refresh()
        {
            Entries.Clear();
            foreach(var ml in Database.Instance.GetMusicList())
            {
                Entries.Add(ml);
            }
            Entries.Add(new AllMusicList());
            Entries.Add(new UnregisteredMusicList());
        }

    }

    
}
