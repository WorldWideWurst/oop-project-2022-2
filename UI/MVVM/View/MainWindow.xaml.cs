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
using Project;
using Project.Music;
using Project.UI.MVVM.ViewModel;

namespace Project.UI
{
    /// Bearbeitet von Philipp Funk und Janek Engel, nur temorär um Kompilierung zu ermöglichen
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //Initialisiert und zeigt Startseite an
            DataContext = new StartseiteViewModel();
            InitializeComponent();
        }

        //Fügt bei Maximiertem Fenster einen Rand hinzu, damit das Fenster im angezeigten Bereich bleibt
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    LayoutRoot.Margin = new Thickness(7, 7, 7, 7);
                    break;
                case WindowState.Normal:
                    LayoutRoot.Margin = new Thickness(0, 0, 0, 0);
                    break;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Went to last song");
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Skipped song");
        }

        private void PlayCheckbox_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("'Never gonna give you up' - Rick Astley");
        }

        private void RandomizeCheckbox_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Randomizing Song Order!");
        }

        private void RepeatCheckbox_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Repeating this song!");
        }

        private void FullscreenCheckbox_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Maximizing!");
        }

        private void LikeCheckbox_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Liked the song!");
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void StartseiteButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new StartseiteViewModel();
        }

        private void PlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new PlaylistsViewModel();
        }

        private void DownloaderButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new DownloaderViewModel();
        }

        private void FileManagerButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new FileManagerViewModel();
        }

        private void CurrentListButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new CurrentListViewModel();
        }
    }
}
