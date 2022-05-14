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
    // Angepasst von Janek Engel
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


        //Knöpfe der Titel-Leiste
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


        //Audio-Player-Buttons
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Went to last song");
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Skipped song");
        }


        private void PlayCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("'Never gonna give you up' - Rick Astley");
        }
        private void PlayCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("'Gave you up' - Ack Ristley");
        }


        private void RandomizeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Randomizing Song Order!");
        }
        private void RandomizeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not randomizing anymore");
        }

        private void RepeatCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Repeating this song!");
        }
        private void RepeatCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not repeating this song anymore");
        }


        private void FullscreenCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Maximizing!");
        }
        private void FullscreenCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Minimizing!");
        }


        private void LikeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Liked the song!");
        }

        private void LikeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Dont like the song anymore :(");
        }


        //Menü-Buttons
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
