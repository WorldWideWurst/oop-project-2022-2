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
            MessageBox.Show("Page");
            Player.Player.Instance.Play()
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Play");
        }

        private void AddToFrontButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddToBackButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
