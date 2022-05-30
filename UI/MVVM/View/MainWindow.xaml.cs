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
using System.Windows.Threading;
using Microsoft.Win32;
using Project;
using Project.Data;
using Project.UI.MVVM.Model;
using Project.UI.MVVM.View;
using Project.UI;
using Project.UI.MVVM.ViewModel;


namespace Project.UI
{
    // Verfasst von Janek Engel
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //Initialisiert und zeigt Startseite an
            DataContext = new Startseite();
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

        
        //Menü-Buttons
        private void StartseiteButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new Startseite();
        }

        private void PlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new Playlists();
        }

        private void DownloaderButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new Downloader();
        }

        private void FileManagerButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new FileManager();
        }

        private void EinstellungenButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new Einstellungen();
        }

        public void FullscreenShow(bool show)
        {
            if(show) 
            { 
                FullscreenView.Visibility = Visibility.Visible;
            }
            else 
            {
                FullscreenView.Visibility = Visibility.Collapsed;
            }
        }
    }
}
