using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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
using Project;
using Project.Download;

namespace Project.UI.MVVM.View
{

    
    /// <summary>
    /// Interaktionslogik für Downloader.xaml
    /// </summary>
    public partial class Downloader : UserControl
    {

        public MusicDownload? CurrentEntry 
        {
            get => currentEntry;
            set
            {
                currentEntry = value;
                DataContext = this;
                if(currentEntry != null)
                {
                    SelectionDisplay.Visibility = Visibility.Visible;
                    EnqueueOptions.Visibility = Visibility.Visible;
                    SelectionDisplay.DataContext = value;

                    NoSelectionDisplay.Visibility = Visibility.Hidden;
                }
                else
                {
                    SelectionDisplay.Visibility = Visibility.Hidden;
                    EnqueueOptions.Visibility = Visibility.Hidden;

                    NoSelectionDisplay.Visibility = Visibility.Visible;
                }
            }
        }
        MusicDownload? currentEntry;

        public Downloader()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void DownloaderButton_Click(object sender, RoutedEventArgs e)
        {
            DoSearch();
        }

        private void DoSearch()
        {
            var uriString = URLInput.Text;
            Uri uri;

            try
            {
                uri = new Uri(uriString);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            // nun kann die eingabe gelöscht werden
            URLInput.Clear();
            HintShow(null, null); // lol

            var musicDownload = Download.Download.Instance.GetMusicDownload(uriString);
            CurrentEntry = musicDownload;

            Download.Download.Instance.DirectDownloadDownload(musicDownload);
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

        private void CancelDownload_Click(object sender, RoutedEventArgs e)
        {
            // Queue.Remove((DownloadEntry)((Button)sender).DataContext);
        }

        private void QueueDisplay_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            var listBox = (ListBox)sender;
            // CurrentEntry = (DownloadEntry?)listBox.SelectedItem;
        }

        private void EnqueueFront_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentEntry != null)
            {
                // Queue.Insert(0, CurrentEntry);
                CurrentEntry = null;
            }
        }

        private void EnqueueBack_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentEntry != null)
            {
                // Queue.Add(CurrentEntry);
                CurrentEntry= null;
            }
        }

    }
}
