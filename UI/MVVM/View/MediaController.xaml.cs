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
    /// <summary>
    /// Interaktionslogik für MediaController.xaml
    /// </summary>
    public partial class MediaController : UserControl
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();

        public MediaController()
        {
            InitializeComponent();
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


        private void LikeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Liked the song!");
        }

        private void LikeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Dont like the song anymore :(");
        }




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


        //Audio-Player Controls

        private void Changed_Slider_Value(object sender, MouseButtonEventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                mediaPlayer.Pause();
                mediaPlayer.Position = (SongSlider.Value * mediaPlayer.NaturalDuration.TimeSpan) / 100;
                PlayCheckbox.IsChecked = true;
                mediaPlayer.Play();
            }
        }

        private void ChooseSong_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                mediaPlayer.Open(new Uri(openFileDialog.FileName));
                PlayCheckbox.IsChecked = false;

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(0.5);
                timer.Tick += timer_Tick;
                timer.Start();
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                lblStatus.Content = String.Format("{0} / {1}", mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
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
