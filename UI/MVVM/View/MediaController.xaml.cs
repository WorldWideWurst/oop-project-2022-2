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
using Project.Player;

namespace Project.UI.MVVM.View
{
    // Verfasst von Janek Engel
    public partial class MediaController : UserControl
    {

        private Player.Player mediaPlayer = new Player.Player();
        private DispatcherTimer timer = new DispatcherTimer();

        public MediaController()
        {
            InitializeComponent();
            OnInitializing();
        }

        private void OnInitializing()
        {
            timer.Interval = TimeSpan.FromSeconds(Tickspeed.tickspeed);
            timer.Tick += timer_Tick;
            timer.Start();
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

        //ruft den Player auf und passt den Playbutton an das Ergebnis an
        private void PlayCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            PlayCheckbox.IsChecked = mediaPlayer.PlaySong() == true ? true : false;
        }
        private void PlayCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            mediaPlayer.PauseSong();
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
            //Funktionalität ist im timer_Tick eingebaut
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
            mediaPlayer.ChangedSliderValue(SongSlider.Value);
        }

        //Startet den MediaPlayer (nach Songauswahl) und legt die Ticklänge fest
        private void ChooseSong_Click(object sender, RoutedEventArgs e)
        {
            PlayCheckbox.IsChecked = mediaPlayer.ChooseSource() == true ? true : false;
        }


        //Fügt dem Volume-Slider das ValueChanged-Event hinzu sobald das erste mal über den Button gehover wird
        private void VolumeButton_MouseEnter(object sender, MouseEventArgs e)
        {

            Slider VolumeSlider = VolumeButton.Template.FindName("ButtonSlider", VolumeButton) as Slider;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
        }

        //Aktualisiert die Lautstärke mit dem Slider-Wert
        private void VolumeSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            Slider VolumeSlider = VolumeButton.Template.FindName("ButtonSlider", VolumeButton) as Slider;
            mediaPlayer.ChangeVolume(VolumeSlider.Value / 100);
        }


        //passiert bei jedem Tick 
        public void timer_Tick(object sender, EventArgs e)
        {
            lblStatus.Content = mediaPlayer.GetLabel();

            if (!SongSlider.IsMouseCaptureWithin)
                SongSlider.Value = mediaPlayer.GetSlider();
        }
    }
}
