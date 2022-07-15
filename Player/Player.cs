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
    /// <summary>
    /// Mögliche Zustände für den Player
    /// </summary>
    public enum PlayerState
    {
        Idle, // Es wird nichts abgespielt.
        Loading, // Musik wird geladen.
        Loaded // Musik ist spielbereit oder wird abgespielt.
    }

    /// <summary>
    /// Mögliche Zustände für den Repeat-Moduls.
    /// NoRepeat: Kein Lied wird wiederholt, die Warteschlange wird abgearbeitet.
    /// RepeatCurrent: Das Aktuelle Lied wird permanent wiederholt.
    /// RepeatQueue: Wenn aktiviert, wird ab dem zu dem 
    /// Zeitpunk spielenden Lied die Warteschlagne bis zum Ende wiederholt.
    /// </summary>
    public enum RepeatState
    {
        NoRepeat,
        RepeatCurrent,
        RepeatQueue,
    }

    /// <summary>
    /// Zentrale Player-Klasse, die das Verwalten des Zustands des Musik-Players.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Unterliegender Player, der das low-level Abspielen von Musik übernimmt.
        /// </summary>
        private readonly MediaPlayer player = new();
        /// <summary>
        /// Sendet ticks aus, die regelmäßig Event Listener benachrichtigen über den Fortschritt des Musikabspielens.
        /// </summary>
        private readonly DispatcherTimer timer = new();


        /// <summary>
        /// Zentrale Player-Instanz.
        /// </summary>
        public static readonly Player Instance = new();


        private Player()
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;

            player.MediaOpened += Player_MediaOpened;
            player.MediaEnded += Player_MediaEnded;

            // dieser Eventhandler sorgt dafür, dass wenn der Player gerade Idle ist und neues zur
            // Warteschlange hinzugefügt wurde auch sofort diese neue Musik lädt.
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

        /// <summary>
        /// Die Warteschlange. Ist eine ObservableCollection, damit Änderungen leichter von
        /// Interessierten abgefangen werden können.
        /// </summary>
        public ObservableCollection<Music> CurrentList { get; } = new ObservableCollection<Music>();

        /// <summary>
        /// Der momentane Index des aktuell gespielten Liedes in der Warteschlange.
        /// </summary>
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

        /// <summary>
        /// Abfrage, ob der Player Idle (faul) ist.
        /// </summary>
        public bool IsIdle => CurrentState == PlayerState.Idle;

        /// <summary>
        /// Genauere Abfrage des aktuellen STatus des Players.
        /// </summary>
        /// <seealso cref="PlayerState"/>
        public PlayerState CurrentState
        {
            get => currentState;
            set => currentState = value;
        }
        PlayerState currentState = PlayerState.Idle;


        /// <summary>
        /// Welches Musikstück atkuell gespielt wird.
        /// </summary>
        public Music? CurrentMusic => IsIdle ? null : CurrentList[CurrentIndex];

        /// <summary>
        /// Ob der Player im Spiel-Moduls ist oder nicht.
        /// Der Spielmodus wird selbst im Idle-Zustand erhalten - heißt, wenn der Player im Idle-Zustand
        /// ist und man ein Lied in die Warteschlange hinzufügt, fängt dieser von selbst an zu spielen.
        /// In diesem Verhalten unterchiedet er sich Beispielsweise von Spotify oder anderen Musik-Playern.
        /// </summary>
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

        /// <summary>
        /// Fortschritt von 0 bis 1 im Lied.
        /// </summary>
        public double PositionRatio => IsIdle ? 0.0 : player.NaturalDuration.HasTimeSpan ? player.Position / player.NaturalDuration.TimeSpan : 0;
        /// <summary>
        /// Zeit-Position im Lied.
        /// </summary>
        public TimeSpan Position => IsIdle ? TimeSpan.Zero : player.Position;
        /// <summary>
        /// Dauer des aktuellen Musikstücks.
        /// </summary>
        public TimeSpan Duration => IsIdle ? TimeSpan.Zero : player.NaturalDuration.HasTimeSpan ? player.NaturalDuration.TimeSpan : player.Position;


        public void Play() => Playing = true;
        public void Pause() => Playing = false;

        /// <summary>
        /// Interne Funktion, um den internen Musikplayer zu beauftragen.
        /// </summary>
        /// <param name="music"></param>
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

        /// <summary>
        /// Der interne Musikplayer wird mit sofortiger wirkung gestoppt.
        /// </summary>
        private void Idle()
        {
            player.Stop();
            timer.Stop();
            CurrentState = PlayerState.Idle;
            WentIdle?.Invoke();
            PlayerTickUpdate?.Invoke();
        }

        /// <summary>
        /// Shuffle Modus. Wenn aktiviert, werden: 1. alle vor der aktuellen Musik vorhandene
        /// Musikstücke gemischt und 2. alle hinzugefügten Musiklisten werden gemischt hinzugefügt.
        /// </summary>
        public bool Shuffle
        {
            get => shuffle;
            set
            {
                shuffle = value;
                if (!value) return;

                // Algorithums kopiert aus StackOverflow
                // mischt den inhalt der CurrentList zufällig
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

        /// <summary>
        /// Repeat-Zustand des Players.
        /// </summary>
        /// <see cref="RepeatState"/>
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


        /// <summary>
        /// Wird immer ausgelöst, wenn ein Tick-Update stattfindet. (standartmäßig alle 1/4 sekunden)
        /// </summary>
        public event Action PlayerTickUpdate;
        /// <summary>
        /// Wird ausgelöst, wenn ein neues Musikstück angespielt wird.
        /// </summary>
        public event Action<Music, int> CurrentMusicChanged;
        /// <summary>
        /// Wird ausgeläst, wenn der Player nicths mehr zu spielen hat und in den Zustand Idle geht.
        /// </summary>
        public event Action WentIdle;






        /// <summary>
        /// Command, um zum nächsten Lied zu wechseln.
        /// Ist der Player im Repeat-Single-Modus, wird einfach das Aktuelle Lied wiederholt.
        /// Ist er im RepeatQueue-Modus und ist er am ender der Warteschlange, beginnt er wieder
        /// an einer Markierten stelle in der Warteschlange zu spielen.
        /// </summary>
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

        /// <summary>
        /// Command, um zum vorherigen Lied zu wechseln.
        /// Ist der Player im Repeat-Single-Modus, wird einfach das Aktuelle Lied wiederholt.
        /// Ist er im RepeatQueue-Modus und ist er an einer markierten STelle in der Warteschlange angelangt,
        /// beginnt er, das letzte lied in der Warteschlange zu spielen (wrap-around)
        /// </summary>
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

        /// <summary>
        /// Setzt den aktuellen Spielfortschritt zurück.
        /// Wiederholt die aktuelle Musik effektiv.
        /// </summary>
        public void RestartMusic()
        {
            player.Position = TimeSpan.Zero;
        }

        /// <summary>
        /// Array zufällig Mischen.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        private static T[] reorderRandom<T>(T[] list)
        {
            var r = new Random();
            return list.OrderBy(e => r.Next()).ToArray();
        }

        /// <summary>
        /// Fügt eine ganze Musikliste hinten an die Warteschlange an,
        /// </summary>
        /// <param name="list"></param>
        public void AppendMusicList(Data.MusicList list)
        {
            var entries = list.MusicEntries.Target;
            if(Shuffle) entries = reorderRandom(entries);
            
            foreach (var music in entries)
            {
                CurrentList.Add(music);
            }
        }

        /// <summary>
        /// fügt eine ganze Musikliste direkt hinter das aktuelle Musikstück ein.
        /// </summary>
        /// <param name="list"></param>
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

        /// <summary>
        /// Fügt ein einzelnes Musikstück direkt hinter das aktuelle ein.
        /// </summary>
        /// <param name="music"></param>
        public void PrependMusic(Music music)
        {
            int offset = CurrentIndex + (IsIdle ? 0 : 1);
            CurrentList.Insert(offset, music);
        }

        /// <summary>
        /// Fügt Musik an das Ende der Warteschlange an.
        /// </summary>
        /// <param name="music"></param>
        public void AppendMusic(Music music)
        {
            CurrentList.Add(music);
        }

        /// <summary>
        /// Entfernt einen Eintrag aus der WArteschlange.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveMusic(int index)
        {
            // repeatMark wenn nötig verschieben
            if(Repeat == RepeatState.RepeatQueue && index < repeatMark)
            {
                repeatMark--;
            }
            // den aktuelle Liedindex auch verschieben falls nötig
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

        /// <summary>
        /// Eine kombinierte Methode, um ein Lied sofort abzuspielen. Klick, Play.
        /// </summary>
        /// <param name="music"></param>
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

        /// <summary>
        /// Lautstärke ändern.
        /// </summary>
        /// <param name="volume"></param>
        public void ChangeVolume(double volume)
        {
            player.Volume = volume;
        }

        /// <summary>
        /// Spiel-Fortschritt setzen.
        /// </summary>
        /// <param name="value"></param>
        public void ChangedSliderValue(double value)
        {
            if (!IsIdle)
            {
                player.Position = value * Duration;
                PlayerTickUpdate?.Invoke();
            }
        }


        /// <summary>
        /// Event tritt bei jedem Tick des Timers auf
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetSlider() => PositionRatio;
    }
}
