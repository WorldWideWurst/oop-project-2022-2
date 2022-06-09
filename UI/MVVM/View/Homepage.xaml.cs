using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using Project.Data;

namespace Project.UI.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für Startseite.xaml
    /// </summary>
    public partial class Homepage : UserControl
    {

        private MediaPlayer mediaPlayer = new MediaPlayer();

        public Homepage()
        {
            InitializeComponent();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
            Download.DownloadFile.Download("https://www.codeproject.com/Questions/5248986/Make-a-WPF-text-box-accept-the-enter-key", ((Project.App)Application.Current).DefaultDownloadFolder)
                .ToString()
                );
        }
    }
}
