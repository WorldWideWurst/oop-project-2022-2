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
    /// Interaktionslogik für Main.xaml
    /// </summary>
    public partial class Main : UserControl, ILibraryPage
    {
        public Main()
        {
            InitializeComponent();
            RepopulatePlaylists();
            Data.Database.Instance.DatabaseChanged += (type, _) => 
            { 
                if (type == typeof(Data.MusicInList) || type == typeof(Data.MusicList)) 
                    Application.Current.Dispatcher.Invoke(() => RepopulatePlaylists()); 
            };
        }


        void RepopulatePlaylists()
        {
            Items.Items.Clear();

            var virtualPlaylists = new Data.IMusicList[]
            {
                new Data.AllMusicList(),
                new Data.UnregisteredMusicList(),
            };
            var allPlaylists = virtualPlaylists.Concat(Data.Database.Instance.GetMusicList());

            foreach(var list in allPlaylists)
            {
                var ctrl = new PlaylistMiniature(list)
                {
                    Margin = new Thickness(0)
                };
                Items.Items.Add(ctrl);
            }
        }
        public void Search(string queryString)
        {
            throw new NotImplementedException();
        }

        public void Overview()
        {
            throw new NotImplementedException();
        }

    }
}
