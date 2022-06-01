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
using Project.UI;
using Project.UI.MVVM.View;

namespace Project.UI.MVVM.View
{
    // Verfasst von Janek Engel
    public partial class MediaController : UserControl
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();

        public MediaController()
        {
            InitializeComponent();
        }

        //Audio-Player-Buttons, noch nicht implementiert
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
            else
                PlayCheckbox.IsChecked = false;
        }
        private void PlayCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        //noch nicht implementiert
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


        private void LikeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Liked the song!");
        }

        private void LikeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Dont like the song anymore :(");
        }



        //Kontrolliert im MainWindow die Sichtbarkeit des Fullscreen-views
        private void FullscreenCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).FullscreenShow(true);
            FullscreenCheckbox.IsChecked = true;
        }
        private void FullscreenCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).FullscreenShow(false);
            FullscreenCheckbox.IsChecked = false;
        }

        //Skippt zur stelle im Lied, die mit dem Slider-Wert übereinstimmt
        private void Changed_Slider_Value(object sender, MouseButtonEventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                mediaPlayer.Position = (SongSlider.Value * mediaPlayer.NaturalDuration.TimeSpan) / 100;
            }
        }

        //Startet den MediaPlayer (nach Songauswahl) und legt die Ticklänge fest
        private void ChooseSong_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                mediaPlayer.Open(new Uri(openFileDialog.FileName));
                PlayCheckbox.IsChecked = false;

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(0.25);
                timer.Tick += timer_Tick;
                timer.Start();
            }
        }

        //Event tritt bei jedem Tick des Timers auf
        void timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                lblStatus.Content = String.Format("{0} / {1}", mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                if(!SongSlider.IsMouseCaptureWithin)
                    SongSlider.Value = mediaPlayer.Position / mediaPlayer.NaturalDuration.TimeSpan * 100;

                if (mediaPlayer.Position == mediaPlayer.NaturalDuration.TimeSpan)
                    if (RepeatCheckbox.IsChecked == true)
                        mediaPlayer.Position = TimeSpan.Zero;
                    else
                        PlayCheckbox.IsChecked = false;
            }
            else
                lblStatus.Content = "Es ist kein Lied ausgewählt!";
        }
    }
}
