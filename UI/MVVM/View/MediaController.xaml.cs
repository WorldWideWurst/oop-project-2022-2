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

        public MediaController()
        {
            InitializeComponent();
            Player.Player.Instance.PlayerTickUpdate += timer_Tick;
            Player.Player.Instance.CurrentMusicChanged += Player_MusicChanged;
            Player.Player.Instance.WentIdle += Player_WentIdle;
        }

        //Audio-Player-Buttons, noch nicht implementiert
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Player.Instance.Position < new TimeSpan(0, 0, 5))
            {
                Player.Player.Instance.PlayPrevious();
            }
            else
            {
                Player.Player.Instance.RestartMusic();
            }
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Player.Instance.PlayNext();
        }

        //ruft den Player auf und passt den Playbutton an das Ergebnis an
        private void PlayCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Player.Player.Instance.Playing = true;
            PlayCheckbox.IsChecked = Player.Player.Instance.Playing;

        }
        private void PlayCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Player.Player.Instance.Playing = false;
            PlayCheckbox.IsChecked = Player.Player.Instance.Playing;
        }

        //noch nicht implementiert
        private void RandomizeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            
        }
        private void RandomizeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }

        private void RepeatCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            
        }
        private void RepeatCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }
        private void RepeatCheckbox_Indeterminant(object sender, RoutedEventArgs e)
        {
            
        }


        private void LikeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Dieses Feature macht leider noch nichts :(");
        }

        private void LikeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            
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
            Player.Player.Instance.ChangedSliderValue(SongSlider.Value / 100);
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
            Player.Player.Instance.ChangeVolume(VolumeSlider.Value / 100);
        }


        //passiert bei jedem Tick 
        public void timer_Tick()
        {
            lblStatus.Content = Player.Player.Instance.GetLabel();

            if (!SongSlider.IsMouseCaptureWithin)
                SongSlider.Value = Player.Player.Instance.GetSlider() * 100;

            PlayCheckbox.IsChecked = Player.Player.Instance.Playing;
            RandomizeCheckbox.IsChecked = Player.Player.Instance.Shuffle;
        }

        //passiert wenn der Song geändert wird 
        public void Player_MusicChanged(Data.Music music, int index)
        {
            if (music.Art.Target != null) Thumbnail.Source = new BitmapImage(new Uri(music.Art.Target.Address));
            if (music.Title != null) SongNameText.Text = music.Title;
            else if (music.Sources.Target.Any()) SongNameText.Text = music.Sources.Target[0].Address.Split("\\").Last().Split(".").First();
            ArtistText.Text = music.Artists.Target.Any() ? new StringBuilder().AppendJoin(", ", music.Artists.Target.Select(mba => mba.ArtistId)).ToString() : "<unknown>";
            AlbumText.Text = music.AlbumName ?? "<unknown>";
        }

        //passiert wenn der Player Idle geht
        public void Player_WentIdle()
        {
            Thumbnail.Source = null;
            SongNameText.Text = null;
            ArtistText.Text = null;
            AlbumText.Text = null;
        }
    }
}
