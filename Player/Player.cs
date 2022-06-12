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
using System.Linq;

namespace Project.Player
{
    public class Player
    {

        private MediaPlayer player = new MediaPlayer();
        private DispatcherTimer timer = new DispatcherTimer();


        public static readonly Player Instance = new Player();

        public Player()
        {
            timer.Interval = TimeSpan.FromSeconds(Tickspeed.tickspeed);
            timer.Tick += timer_Tick;
            timer.Start();
            player.MediaOpened += Player_MediaOpened;
            player.MediaEnded += Player_MediaEnded;
        }

        public IList<Music> CurrentList { get; } = new List<Music>();

        public int CurrentIndex
        {
            get
            {
                return currentIndex;
            }
            set
            {

                if (value < 0)
                {
                    value = 0;
                }

                if (value < CurrentList.Count)
                {
                    currentIndex = value;
                    LoadMusic(CurrentMusic);
                }
                else
                {
                    currentIndex = CurrentList.Count;
                    Idle();
                }
            }
        }

        private int currentIndex;

        public bool IsIdle => CurrentIndex == CurrentList.Count;


        public Music? CurrentMusic => IsIdle ? null : CurrentList[CurrentIndex];

        public bool Playing
        {
            get => playing;
            set
            {
                playing = value;
                if(value)
                {
                    if(!IsIdle)
                    {
                        player.Play();
                    }
                }
                else
                {
                    if(!IsIdle)
                    {
                        player.Pause();
                    }
                }
            }
        }

        private bool playing = false;

        public double PositionRatio => IsIdle ? 0.0 : player.NaturalDuration.HasTimeSpan ? player.Position / player.NaturalDuration.TimeSpan : 0;
        public TimeSpan Position => IsIdle ? TimeSpan.Zero : player.Position;
        public TimeSpan Duration => IsIdle ? TimeSpan.Zero : player.NaturalDuration.HasTimeSpan ? player.NaturalDuration.TimeSpan : player.Position;


        public void Play() => Playing = true;
        public void Pause() => Playing = false;

        private void LoadMusic(Music music)
        {
            var source = music.Sources.First();
            mediaLoaded = false;
            player.Open(new Uri(source.Address));
        }

        private void Idle()
        {
            player.Stop();
        }


        private bool mediaLoaded = false;


        public bool Shuffle
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }


        public event EventHandler CurrentListChanged;










        public void PlayNext()
        {
            CurrentIndex++;
        }

        public void PlayPrevious()
        {
            CurrentIndex--;
        }

        public void RestartMusic()
        {
            player.Position = TimeSpan.Zero;
        }

        public bool ChooseSource()
        {
            bool status;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                player.Open(new Uri(openFileDialog.FileName));
                status = true;
            }
            else
                status = false;
            return status;
        }

        public void AppendMusicList(Data.MusicList list)
        {
            var wasIdle = IsIdle;
            foreach (var music in list.MusicEntries)
            {
                CurrentList.Add(music);
            }
            if(CurrentListChanged != null) CurrentListChanged(this, EventArgs.Empty);
            if (player.Source == null)
            {
                LoadMusic(CurrentMusic);
            }
        }

        public void PrependMusicList(MusicList list)
        {
            var entries = list.MusicEntries.ToList();
            int extra = CurrentIndex == CurrentList.Count ? 0 : 1;
            for (int i = 0; i < entries.Count; i++)
            {
                CurrentList.Insert(CurrentIndex + i + extra, entries[i]);
            }
            if (CurrentListChanged != null) CurrentListChanged(this, EventArgs.Empty);
            if (player.Source == null)
            {
                LoadMusic(CurrentMusic);
            }
        }

        public void PrependMusic(Music music)
        {
            int extra = CurrentIndex == CurrentList.Count ? 0 : 1;
            CurrentList.Insert(CurrentIndex + extra, music);
            if (CurrentListChanged != null) CurrentListChanged(this, EventArgs.Empty);
            if (player.Source == null)
            {
                LoadMusic(CurrentMusic);
            }
        }

        public void AppendMusic(Music music)
        {
            CurrentList.Add(music);
            if (CurrentListChanged != null) CurrentListChanged(this, EventArgs.Empty);
            if(player.Source == null)
            {
                LoadMusic(CurrentMusic);
            }
        }

        public void PlayImmediately(Music music)
        {
            if(IsIdle)
            {
                AppendMusic(music);
            }
            else
            {
                PrependMusic(music);
                PlayNext();
            }
        }


        public void ChangeVolume(double volume)
        {
            player.Volume = volume;
        }

        public void ChangedSliderValue(double value)
        {
            if (!IsIdle)
            {
                player.Position = value * Duration;
            }
        }


        //Event tritt bei jedem Tick des Timers auf
        private void timer_Tick(object sender, EventArgs e)
        {
            if (!IsIdle)
            {
                if (timer.Interval != TimeSpan.FromSeconds(Tickspeed.tickspeed))
                    timer.Interval = TimeSpan.FromSeconds(Tickspeed.tickspeed);
            }
        }

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            PlayNext();
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            mediaLoaded = true;
            if (Playing)
            {
                player.Play();
            }
            else
            {
                player.Pause();
            }
        }

        public string GetLabel()
         {
             string text;
             if(IsIdle)
             {
                 text = "Es wurde keine Musik ausgewählt!";
             }
             else if(!mediaLoaded)
             {
                 text = "Musik wird geladen...";
             }
             else
             {
                 string format;
                 if(Duration >= new TimeSpan(1, 0, 0))
                 {
                     format = @"hh\:mm\:ss";
                 }
                 else if(Duration >= new TimeSpan(0, 1, 0))
                 {
                     format = @"mm\:ss";
                 }
                 else
                 {
                     format = @"'00:'ss\.fff";
                 }
                 var dur = Duration.ToString(format);
                 var pos = Position.ToString(format);
                 text = $"{pos} / {dur}";
             }
             return text;
         }

         public double GetSlider() => PositionRatio;
    }
}
