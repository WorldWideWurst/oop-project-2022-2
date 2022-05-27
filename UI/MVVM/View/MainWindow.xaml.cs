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
using Project.UI.MVVM.ViewModel;


namespace Project.UI
{
    // Angepasst von Janek Engel
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();

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
            if (mediaPlayer.Source != null)
                mediaPlayer.Play();
        }
        private void PlayCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
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
        private void RepeatCheckbox_Indeterminant(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Repeating the Playlist");
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


        private void Changed_Slider_Value(object sender, MouseButtonEventArgs e)
        {
            if (mediaPlayer.Source != null) {
                mediaPlayer.Pause();
                mediaPlayer.Position = (SongSlider.Value * mediaPlayer.NaturalDuration.TimeSpan) / 100;
                PlayCheckbox.IsChecked = true;
                mediaPlayer.Play();
            }
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

        private void EinstellungenButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new EinstellungenViewModel();
        }

        private void ChooseSong_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true) { 
                mediaPlayer.Open(new Uri(openFileDialog.FileName));
                PlayCheckbox.IsChecked = false;

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += timer_Tick;
                timer.Start();
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null) { 
                lblStatus.Content = String.Format("{0} / {1}", mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                SongSlider.Value = mediaPlayer.Position / mediaPlayer.NaturalDuration.TimeSpan * 100; 
                
                if (mediaPlayer.Position == mediaPlayer.NaturalDuration.TimeSpan)
                    if(RepeatCheckbox.IsChecked == true)
                        mediaPlayer.Position = TimeSpan.Zero;
                    else
                        PlayCheckbox.IsChecked = false;
            }
            else
                lblStatus.Content = "Es ist kein Lied ausgewählt!";
        }
    }
}
