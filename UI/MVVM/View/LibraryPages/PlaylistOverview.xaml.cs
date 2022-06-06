using Project.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project.UI.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für Playlist.xaml
    /// </summary>
    public partial class PlaylistOverview : UserControl, LibraryPages.ILibraryPage
    {

        public readonly MusicList MusicList;

        public PlaylistOverview(MusicList musicList)
        {
            InitializeComponent();
            MusicList = musicList;

            PlaylistName.Text = musicList.Name;
            foreach(var entry in musicList.MusicEntries)
            {
                EntryList.Items.Add(entry.Title);
            }
        }

        public void Overview()
        {
            throw new NotImplementedException();
        }

        public void Search(string queryString)
        {
            throw new NotImplementedException();
        }
    }
}
