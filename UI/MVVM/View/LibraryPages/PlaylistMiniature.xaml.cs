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

// verfasst von Richard Förster

namespace Project.UI.MVVM.View.LibraryPages
{
    /// <summary>
    /// Interaktionslogik für PlaylistMiniature.xaml
    /// </summary>
    public partial class PlaylistMiniature : UserControl
    {

        public PlaylistMiniature() 
        {
            InitializeComponent();
        }

        public PlaylistMiniature(Data.IMusicList musicList) : this()
        {
            DataContext = musicList;
        }

        private void GotoPlaylist_Click(object sender, RoutedEventArgs e)
        {
            PlaylistOverview ctrl = new((Data.IMusicList)DataContext);
            ((MainWindow)Application.Current.MainWindow).OpenLibraryPage(ctrl);
        }
    }
}
