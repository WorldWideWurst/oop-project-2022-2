using System;
using System.Collections.Generic;
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

namespace Project.UI.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für Downloader.xaml
    /// </summary>
    public partial class Downloader : UserControl
    {
        public Downloader()
        {
            InitializeComponent();
        }

        private void DownloaderButton_Click(object sender, RoutedEventArgs e)
        {
            Results.Items.Clear();
            var targetDir = ((App)Application.Current).DefaultDownloadFolder;
            foreach(var uriStr in URLInput.Text.Split('\n').Select(s => s.Trim()))
            {
                var uri = new Uri(uriStr);
                var tempPath = System.IO.Path.GetTempFileName();
                if(!Download.DownloadFile.Download(uriStr, tempPath))
                {
                    MessageBox.Show("Fehler beim Herunterladen von URI " + uri);
                    return;
                }
                

                string[] innerURIs = Download.DownloadFile.ParseForDownloadables(uriStr);
                var anySuccessfulURIs = false;
                foreach(var innerURIString in innerURIs) { 
                    try
                    {
                        var innerURI = new Uri(innerURIString);
                        if(Download.DownloadFile.Download(innerURIString, System.IO.Path.Combine(targetDir, fileNameForURI(innerURI))))
                        {
                            anySuccessfulURIs = true;
                        }
                    }
                    catch(UriFormatException)
                    {
                        
                    }
                }

                if(!anySuccessfulURIs)
                {
                    File.Move(tempPath, System.IO.Path.Combine(targetDir, fileNameForURI(uri)));
                }
                ShowResult(uriStr);
            }
            URLInput.Text = "";
        }

        private static string fileNameForURI(Uri uri)
        {
            var name = System.IO.Path.GetFileName(uri.AbsolutePath);
            if (name != null && name.Length > 0)
            {
                return name;
            }
            else
            {
                return $"download_{Math.Abs(uri.ToString().GetHashCode())}";
            }
        }

        private void ShowResult(string uriStr)
        {
            
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
    }
}
