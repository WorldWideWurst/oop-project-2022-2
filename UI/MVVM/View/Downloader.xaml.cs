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
            DownloadFile.ParseForDownloadables(URL.Text);
        }


    }
}
