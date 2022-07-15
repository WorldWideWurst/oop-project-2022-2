using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using Project;
using Project.Download;

//erstellt von Richard Förster

namespace Project.UI.MVVM.View
{

    public enum DownloadStage
    {
        Waiting,
        Enqueued,
        Downloading,
        Finished,
        Aborted,
        Error,
    }

    public class DownloadEntry : INotifyPropertyChanged
    {
        public DownloadStage Stage
        {
            get => stage;
            set { stage = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Stage))); }
        }
        private DownloadStage stage;
        public MusicDownloadInfo? Info { get; set; }
        public string Target { get; set; }
        public float Progress
        {
            get => progress;
            set { progress = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress))); }
        }
        private float progress;
        public DownloadSettings Settings { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// Interaktionslogik für Downloader.xaml
    /// </summary>
    public partial class Downloader : UserControl, INotifyPropertyChanged
    {

        public DownloadEntry? CurrentEntry 
        {
            get => currentEntry;
            set
            {
                currentEntry = value;
                if(currentEntry != null)
                {
                    SelectionDisplay.Visibility = Visibility.Visible;
                    SelectionDisplay.DataContext = value;

                    NoSelectionDisplay.Visibility = Visibility.Hidden;
                }
                else
                {
                    SelectionDisplay.Visibility = Visibility.Hidden;

                    NoSelectionDisplay.Visibility = Visibility.Visible;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentEntry)));
                UpdateButtonAvailability();
            }
        }
        DownloadEntry? currentEntry;

        public ObservableCollection<DownloadEntry> Queue { get; set; } = new ObservableCollection<DownloadEntry>();
        public ObservableCollection<DownloadEntry> Done { get; set; } = new ObservableCollection<DownloadEntry>();

        public bool AutoDownloadEnabled
        {
            get => autoDownloadEnabled;
            set
            {
                autoDownloadEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoDownloadEnabled)));
                if(value)
                {
                    downloadNext();
                }
            }
        }
        private bool autoDownloadEnabled;

        private Task? currentDownloadProcess = null;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Downloader()
        {
            InitializeComponent();
            DataContext = this;
            AutoDownloadEnabled = true;
            Queue.CollectionChanged += (c, e) =>
            {
                UpdateButtonAvailability();
                downloadNext();
            };
        }

        private void DownloaderButton_Click(object sender, RoutedEventArgs e)
        {
            DoSearch();
        }

        private void DoSearch()
        {
            var uriString = URLInput.Text;

            try
            {
                new Uri(uriString);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }


            var downloadInfo = Download.Download.Instance.GetMusicDownloadInfo(uriString);
            if(downloadInfo == null)
            {
                MessageBox.Show("Konnte keine Informationen über den Link sammeln.");
                return;
            }

            var downloadEntry = new DownloadEntry()
            {
                Info = downloadInfo,
                Target = $"{Download.Download.Instance.DataDownloadPath}\\{downloadInfo.Title}.mp3",
                Progress = 0f,
                Stage = DownloadStage.Waiting,
                Settings = new DownloadSettings()
                {
                    Quality = QualitySetting.Default,
                    DownloadSpeedLimit = 1024 * 1024 * 1024,
                },
            };
            
            CurrentEntry = downloadEntry;

            // nun kann die eingabe gelöscht werden
            URLInput.Clear();
            HintShow(null, null); // lol
        }


        private void HintNull(object sender, RoutedEventArgs e)
        {
            if(URLInput.Text == "Hier einen Link eingeben!")
            {
                URLInput.Text = null;
                URLInput.Foreground = Brushes.Black;
            }
        }

        private void HintShow(object sender, RoutedEventArgs e)
        {
            if(URLInput.Text == "")
            {
                URLInput.Foreground = Brushes.Gray;
                URLInput.Text = "Hier einen Link eingeben!";
            }
        }
        private void URLInput_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                DoSearch();
            }
        }


        private void QueueDisplay_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            var listBox = (ListBox)sender;
            CurrentEntry = (DownloadEntry?)listBox.SelectedItem;
        }

        private void QualitySetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(CurrentEntry != null && e.AddedItems.Count > 0)
            {
                CurrentEntry.Settings.Quality = (string)((ComboBoxItem)e.AddedItems[0]).Content switch
                {
                    "Schlechteste" => QualitySetting.Lowest,
                    "Standard" => QualitySetting.Default,
                    "Beste" => QualitySetting.Best,
                };
            }
        }

        private void UpdateButtonAvailability()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAbortDownload)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanInstantDownload)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanEnqueueDownload)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanDeleteDownload)));
        }

        public bool CanAbortDownload => CurrentEntry != null && Queue.Count > 0 && CurrentEntry.Stage != DownloadStage.Downloading;

        private void AbortDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentEntry != null && CurrentEntry.Stage != DownloadStage.Downloading)
            {
                CurrentEntry.Stage = DownloadStage.Aborted;
                Queue.Remove(CurrentEntry);
                Done.Add(CurrentEntry);
                CurrentEntry = null;
            }
        }

        public bool CanInstantDownload => CurrentEntry != null && (CurrentEntry.Stage == DownloadStage.Waiting || CurrentEntry.Stage == DownloadStage.Enqueued);

        private void InstantDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentEntry != null && (CurrentEntry.Stage == DownloadStage.Waiting || CurrentEntry.Stage == DownloadStage.Enqueued))
            {
                if(Queue.Contains(CurrentEntry))
                {
                    Queue.Move(Queue.IndexOf(CurrentEntry), 0);
                }
                else
                {
                    Queue.Insert(0, CurrentEntry);                
                }
                downloadNext();
            }
        }

        public bool CanDeleteDownload => CurrentEntry != null && CurrentEntry.Stage != DownloadStage.Downloading;

        private void DeleteDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentEntry != null && CurrentEntry.Stage != DownloadStage.Downloading)
            {
                Queue.Remove(CurrentEntry);
                CurrentEntry = null;
            }
        }

        public bool CanEnqueueDownload => CurrentEntry != null && (CurrentEntry.Stage == DownloadStage.Waiting || CurrentEntry.Stage == DownloadStage.Enqueued);

        private void EnqueueDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentEntry != null)
            {
                if(CurrentEntry.Stage == DownloadStage.Waiting)
                {
                    Queue.Add(CurrentEntry);
                }
                else if(CurrentEntry.Stage == DownloadStage.Enqueued)
                {
                    Queue.Remove(CurrentEntry);
                    Queue.Add(CurrentEntry);
                }
            }
        }

        private void FinishedDownload(DownloadEntry entry)
        {
            Queue.Remove(entry);
            entry.Stage = DownloadStage.Finished;
            Done.Add(entry);
        }

        private void ErrorDownload(DownloadEntry entry)
        {
            Queue.Remove(entry);
            entry.Stage = DownloadStage.Error;
            Done.Add(entry);
        }


        private class DownloadProgressObserver : IObserver<float>
        {

            private DownloadEntry entry;
            private Downloader downloader;

            public DownloadProgressObserver(Downloader downloader, DownloadEntry entry)
            {
                this.entry = entry;
                this.downloader = downloader;
            }

            public void OnCompleted()
            {
                Application.Current.Dispatcher.Invoke(() => downloader.FinishedDownload(entry));
            }

            public void OnError(Exception error)
            {
                Application.Current.Dispatcher.Invoke(() => downloader.ErrorDownload(entry));
            }

            public void OnNext(float value)
            {
                entry.Progress = value;
            }
        }

        private Task? downloadNext()
        {
            if(currentDownloadProcess != null)
            {
                return currentDownloadProcess;
            }

            if(Queue.Count == 0)
            {
                return null;
            }

            return currentDownloadProcess = Task.Run(() =>
            {
                var e = Queue[0];
                e.Stage = DownloadStage.Downloading;
                Download.Download.Instance.DownloadMusic(e.Info.Source, e.Target, e.Settings, new DownloadProgressObserver(this, e));
                if(AutoDownloadEnabled)
                {
                    downloadNext(); // funny loop
                }
            });
        }

        private void HelpButton_MouseEnter(object sender, RoutedEventArgs e)
        {
            Help.Visibility = Visibility.Visible;
        }

        private void HelpButton_MouseLeave(object sender, RoutedEventArgs e)
        {
            Help.Visibility = Visibility.Collapsed;
        }
    }
}
