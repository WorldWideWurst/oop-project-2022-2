using System;
using System.Windows.Media;
using Project.Data;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Forms;
using Project.UI.MVVM.View;
using Project.UI;
using System.Windows.Controls;
using System.Windows;

namespace Project.Player
{
    public class Player : MediaPlayer
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private DispatcherTimer timer = new DispatcherTimer();

        public IList<Music>? CurrentList
        {
            get => _musicList;
            set
            {
                if(value == null)
                {
                    // player stoppen
                    _musicList = null;
                    CurrentIndex = null;
                } 
                else
                {   
                    // neue liste wird abgespielt
                    _musicList = value;
                    CurrentIndex = 0;
                }
            }
        }

        private IList<Music>? _musicList;

        public int? CurrentIndex { get; set; }

        public bool PlaySong()
        {
            bool status;
            if (mediaPlayer.Source != null)
            {
                mediaPlayer.Play();
                status = true;
            }
            else
                status = false;
            return status;
        }

        public void PauseSong()
        {
            mediaPlayer.Pause();
        }

        public bool Shuffle
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }


        public void PlayNext()
        {
            throw new NotImplementedException();
        }

        public void PlayPrevious()
        {
            throw new NotImplementedException();
        }

        public bool ChooseSource()
        {
            bool status;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                mediaPlayer.Open(new Uri(openFileDialog.FileName));
                timer.Interval = TimeSpan.FromSeconds(Tickspeed.tickspeed);
                timer.Tick += timer_Tick;
                timer.Start();
                status = true;
            }
            else
                status = false;
            return status;
        }

        public void ChangeVolume(double volume)
        {
            mediaPlayer.Volume = volume;
        }

        public void ChangedSliderValue(double value)
        {
            if (mediaPlayer.Source != null)
            {
                mediaPlayer.Position = (value * mediaPlayer.NaturalDuration.TimeSpan) / 100;
            }
        }


        //Event tritt bei jedem Tick des Timers auf
        private void timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                if (TimeSpan.FromSeconds(Tickspeed.tickspeed) != timer.Interval)
                    timer.Interval = TimeSpan.FromSeconds(Tickspeed.tickspeed);
            }
        }

        public string GetLabel()
        {
            string text;
            if (mediaPlayer.Source != null)
                text = String.Format("{0} / {1}", mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
            else
                text = "Es wurde kein Lied ausgewählt!";
            return text;
        }

        public double GetSlider()
        {
            double value = 0;

            if (mediaPlayer.Source != null)
                value = mediaPlayer.Position / mediaPlayer.NaturalDuration.TimeSpan * 100;
            return value;
        }
    }
}
