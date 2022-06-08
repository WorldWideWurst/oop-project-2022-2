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

        Homepage homepageState = new();
        public Library LibraryTab { get; private set; } = new();
        public MediaController MediaControllerTab { get; private set; } = new();
        Downloader downloaderState = new();
        MusicImporter musicImporterState = new();
        Settings settingsState = new();

        //Instanziiert eine globale Instanz der Player-Klasse
        public static readonly Player.Player playerInstance = new Player.Player();

        public MainWindow()
        {
            InitializeComponent();
            //Initialisiert und zeigt Startseite an
            DataContext = new Homepage();
            MediaControllerViewer.Content = MediaControllerTab;

            // welche lieder sind in einer Playlist?
            // var musicListEntries = Database.Instance.GetMusicList(Guid.Empty).MusicEntries.ToList();
            
            // Die verknüpfte musik einer Musikdatei
            // Database.Instance.GetMusic(Database.Instance.GetSource("asdfasdf").MusicId);


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
            homepageState = DataContext is Homepage? new Homepage() : homepageState;
            DataContext = homepageState;
        }

        private void BibliothekButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLibraryPage(null);
        }

        public void OpenLibraryPage(MVVM.View.LibraryPages.ILibraryPage? page)
        {
            LibraryTab = DataContext is Library ? new Library() : LibraryTab;

            LibraryTab.ShowLibraryPage(page);

            DataContext = LibraryTab;
            
        }

        private void DownloaderButton_Click(object sender, RoutedEventArgs e)
        {
            downloaderState = DataContext is Downloader ? new Downloader() : downloaderState;
            DataContext = downloaderState;
        }

        private void MusicImporterButton_Click(object sender, RoutedEventArgs e)
        {
            musicImporterState = DataContext is MusicImporter ? new MusicImporter() : musicImporterState;
            DataContext = musicImporterState;
        }

        private void EinstellungenButton_Click(object sender, RoutedEventArgs e)
        {
            settingsState = DataContext is Settings ? new Settings() : settingsState;
            DataContext = settingsState;
        }

        public void FullscreenShow(bool show)
        {
            FullscreenView.Visibility = show == true ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
