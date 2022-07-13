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

//erstellt von Richard Förster

namespace Project.UI.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für PlaylistDisplay.xaml
    /// </summary>
    public partial class PlaylistDisplay : UserControl
    {
        public PlaylistDisplay()
        {
            InitializeComponent();
            ListView.ItemsSource = Player.Library.Instance.Entries;
        }

        private void ViewMusicList_Click(object sender, RoutedEventArgs e)
        {
            PlaylistOverview ctrl = new((Data.IMusicList)((Button)sender).DataContext);
            ((MainWindow)Application.Current.MainWindow).OpenLibraryPage(ctrl);
        }
    }
}
