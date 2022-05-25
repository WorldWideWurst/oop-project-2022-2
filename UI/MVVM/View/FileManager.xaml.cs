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
using Project.Index;
using Project.Data;

namespace Project.UI.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für FileManager.xaml
    /// </summary>
    public partial class FileManager : UserControl
    {
        public FileManager()
        {
            InitializeComponent();
        }

        private void SelectMusicDir_Click(object sender, RoutedEventArgs e)
        {
            var dir = MusicDirInput.Text;
            if(!Directory.Exists(dir))
            {
                ImportReport.Text = "Ist kein Ordner.";

                return;
            }

            var allFiles = SimpleDiskIndexer.Instance.Index(dir).ToList();
            foreach(var file in allFiles)
            {
                try
                {
                    var music = Database.Instance.RegisterMusicSource(file);
                } catch (UnsupportedMusicFormat)
                {
                    continue;
                }
            }
            ImportReport.Text = $"${allFiles.Count} Musikdatei(n) registriert.";
        }
    }
}
