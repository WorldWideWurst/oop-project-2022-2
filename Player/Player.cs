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
using System.Collections.ObjectModel;

namespace Project.Player
{

    public enum PlayerState
    {
        Idle,
        Loading,
        Loaded
    }

    public class Player
    {

        private readonly MediaPlayer player = new();
        private readonly DispatcherTimer timer = new();


        public static readonly Player Instance = new();


        private Player()
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;

            player.MediaOpened += Player_MediaOpened;
            player.MediaEnded += Player_MediaEnded;

            CurrentList.CollectionChanged += (sender, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        if(IsIdle)
                        {
                            LoadMusic((Music)e.NewItems[0]);
                        }
                        break;
                }
            };
        }

        public ObservableCollection<Music> CurrentList { get; } = new ObservableCollection<Music>();

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

        public bool IsIdle => CurrentState == PlayerState.Idle;

        public PlayerState CurrentState
        {
            get => currentState;
            set
            {
                currentState = value;
            }
        }
        PlayerState currentState = PlayerState.Idle;


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
            CurrentState = PlayerState.Loading;
            player.Open(new Uri(source.Address));
            timer.Start();
            CurrentMusicChanged?.Invoke(music, CurrentIndex);
            PlayerTickUpdate?.Invoke();
        }

        private void Idle()
        {
            player.Stop();
            timer.Stop();
            CurrentState = PlayerState.Idle;
            WentIdle?.Invoke();
            PlayerTickUpdate?.Invoke();
        }


        public bool Shuffle { get; set; }


        public TimeSpan Tickspeed
        {
            get => timer.Interval;
            set => timer.Interval = value;
        }



        public event Action PlayerTickUpdate;
        public event Action<Music, int> CurrentMusicChanged;
        public event Action WentIdle;







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
            OpenFileDialog openFileDialog = new();
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

        private static T[] reorderRandom<T>(T[] list)
        {
            var r = new Random();
            return list.OrderBy(e => r.Next()).ToArray();
        }

        public void AppendMusicList(Data.MusicList list)
        {
            var entries = list.MusicEntries.ToArray();
            if(Shuffle) entries = reorderRandom(entries);
            
            foreach (var music in entries)
            {
                CurrentList.Add(music);
            }
        }

        public void PrependMusicList(MusicList list)
        {
            var entries = list.MusicEntries.ToArray();
            if(Shuffle) entries = reorderRandom(entries);
            int offset = CurrentIndex + (IsIdle ? 0 : 1);

            for (int i = 0; i < entries.Length; i++)
            {
                CurrentList.Insert(offset + i, entries[i]);
            }
        }

        public void PrependMusic(Music music)
        {
            int offset = CurrentIndex + (IsIdle ? 0 : 1);
            CurrentList.Insert(offset, music);
        }

        public void AppendMusic(Music music)
        {
            CurrentList.Add(music);
        }

        public void PlayImmediately(Music music)
        {
            if(IsIdle)
            {
                AppendMusic(music);
            }
            else
            {
                if(CurrentMusic.Id == music.Id)
                {
                    RestartMusic();
                }
                else
                {
                    PrependMusic(music);
                    PlayNext();
                }
            }
            Playing = true;
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
                PlayerTickUpdate?.Invoke();
            }
        }


        //Event tritt bei jedem Tick des Timers auf
        private void timer_Tick(object sender, EventArgs e)
        {
            PlayerTickUpdate?.Invoke();
        }

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            PlayNext();
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            CurrentState = PlayerState.Loaded;
            if (Playing)
            {
                player.Play();
            }
            else
            {
                player.Pause();
            }
            PlayerTickUpdate?.Invoke();
        }

        public string GetLabel()
         {
             string text;
             if(CurrentState == PlayerState.Idle)
             {
                 text = "Es wurde keine Musik ausgewählt!";
             }
             else if(CurrentState == PlayerState.Loading)
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
