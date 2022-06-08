using Project.Data;
using Project.UI.MVVM.View.LibraryPages;
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

        public PlaylistOverview(IMusicList musicList)
        {
            InitializeComponent();

            DataContextChanged += PlaylistOverview_DataContextChanged;

            DataContext = musicList;
        }

        private void PlaylistOverview_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            EntryTable.Children.Clear();
            foreach(var music in ((IMusicList)e.NewValue).Entries)
            {
                EntryTable.Children.Add(new MusicRecord(music));
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
