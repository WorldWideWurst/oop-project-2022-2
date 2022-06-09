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

namespace Project.UI.MVVM.View.LibraryPages
{
    /// <summary>
    /// Interaktionslogik für PlaylistMiniature.xaml
    /// </summary>
    public partial class PlaylistMiniature : UserControl
    {
        public PlaylistMiniature(Data.IMusicList musicList)
        {
            InitializeComponent();
            DataContext = musicList;
            ArtistName.Text = musicList.Owner;
            EntryCount.Text = musicList.Count.ToString();
        }

        private void GotoPlaylist_Click(object sender, RoutedEventArgs e)
        {
            PlaylistOverview ctrl = new((Data.IMusicList)DataContext);
            ((MainWindow)Application.Current.MainWindow).LibraryTab.ShowLibraryPage(ctrl);
        }
    }
}
