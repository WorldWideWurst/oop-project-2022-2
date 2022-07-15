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

// verfasst von Richard Förster

namespace Project.Player
{

    public enum PlayerState
    {
        Idle,
        Loading,
        Loaded
    }

    public enum RepeatState
    {
        NoRepeat,
        RepeatCurrent,
        RepeatQueue,
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
                    if(value < repeatMark)
                    {
                        repeatMark = value;
                    }
                    LoadMusic(CurrentList[value]);
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
            set => currentState = value;
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
            var source = music.Sources.Target[0];
            var uri = new Uri(source.Address);
            bool isPreloaded = uri == player.Source;

            CurrentState = PlayerState.Loading;
            player.Open(uri);
            timer.Start();
            CurrentMusicChanged?.Invoke(music, CurrentIndex);
            PlayerTickUpdate?.Invoke();
            
            if(isPreloaded)
                Player_MediaOpened(this, EventArgs.Empty);
        }

        private void Idle()
        {
            player.Stop();
            timer.Stop();
            CurrentState = PlayerState.Idle;
            WentIdle?.Invoke();
            PlayerTickUpdate?.Invoke();
        }


        public bool Shuffle
        {
            get => shuffle;
            set
            {
                shuffle = value;
                if (!value) return;

                // Algorithums kopiert aus StackOverflow
                var rng = new Random();
                int offset = CurrentIndex + 1;
                int n = CurrentList.Count - offset;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    var temp = CurrentList[k + offset];
                    CurrentList[k + offset] = CurrentList[n + offset];
                    CurrentList[n + offset] = temp;
                }
            }
        }
        private bool shuffle = false;

        public RepeatState Repeat
        {
            get => repeat;
            set
            {
                if(value == RepeatState.RepeatQueue)
                {
                    repeatMark = CurrentIndex;
                }
                else if(value == RepeatState.NoRepeat)
                {
                    repeatMark = -1;
                }
                repeat = value;
            }
        }
        private RepeatState repeat = RepeatState.NoRepeat;
        private int repeatMark = -1;

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
            if(Repeat == RepeatState.RepeatCurrent)
            {
                RestartMusic();
            }
            else if(Repeat == RepeatState.RepeatQueue && CurrentIndex == CurrentList.Count - 1)
            {
                if(repeatMark > CurrentIndex)
                {
                    repeatMark = CurrentIndex;
                }
                CurrentIndex = repeatMark;
            }
            else
            {
                CurrentIndex++;
            }
        }

        public void PlayPrevious()
        {
            if (Repeat == RepeatState.RepeatCurrent)
            {
                RestartMusic();
            }
            else if (Repeat == RepeatState.RepeatQueue && CurrentIndex == repeatMark)
            {
                CurrentIndex = CurrentList.Count - 1;
            }
            else
            {
                CurrentIndex--;
            }
        }

        public void RestartMusic()
        {
            player.Position = TimeSpan.Zero;
        }

        private static T[] reorderRandom<T>(T[] list)
        {
            var r = new Random();
            return list.OrderBy(e => r.Next()).ToArray();
        }

        public void AppendMusicList(Data.MusicList list)
        {
            var entries = list.MusicEntries.Target;
            if(Shuffle) entries = reorderRandom(entries);
            
            foreach (var music in entries)
            {
                CurrentList.Add(music);
            }
        }

        public void PrependMusicList(MusicList list)
        {
            var entries = list.MusicEntries.Target;
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

        public void RemoveMusic(int index)
        {
            if(index < CurrentIndex)
            {
                currentIndex--;
            }
            CurrentList.RemoveAt(index);
            if (index == CurrentIndex)
            {
                CurrentIndex = index;
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

        public double GetSlider() => PositionRatio;
    }
}
