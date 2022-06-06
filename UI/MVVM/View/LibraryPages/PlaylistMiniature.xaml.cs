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

        public Data.MusicList MusicList { get; private set; }
        public PlaylistMiniature(Data.MusicList musicList)
        {
            InitializeComponent();
            MusicList = musicList;
            PlaylistName.Text = musicList.Name;
            ArtistName.Text = musicList.Owner;
        }

        private void GotoPlaylist_Click(object sender, RoutedEventArgs e)
        {
            PlaylistOverview ctrl = new(MusicList);
            ((MainWindow)Application.Current.MainWindow).LibraryTab.ShowLibraryPage(ctrl);
        }
    }
}
