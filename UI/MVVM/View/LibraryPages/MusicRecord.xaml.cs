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
using Project.UI;
using Project.Player;
using Project.Data;

namespace Project.UI.MVVM.View.LibraryPages
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class MusicRecord : UserControl
    {
        public MusicRecord(Data.Music music)
        {
            InitializeComponent();

            DataContextChanged += MusicRecord_DataContextChanged;

            DataContext = music;
        }

        private void MusicRecord_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var music = (Data.Music)e.NewValue;
            
            var artists = music.Artists;
            Artists.Children.Clear();
            foreach (var artist in artists)
            {
                Artists.Children.Add(new TextBlock()
                {
                    Text = artist.ArtistId
                });
            }
        }

        private void OpenPageButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new MusicOverview((Music)DataContext);
            ((MainWindow)Application.Current.MainWindow).LibraryTab.ShowLibraryPage(page);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Player.Instance.PlayImmediately((Music)DataContext);
        }

        private void AddToFrontButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Player.Instance.PrependMusic((Music)DataContext);
        }

        private void AddToBackButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Player.Instance.AppendMusic((Music)DataContext);
        }
    }
}
