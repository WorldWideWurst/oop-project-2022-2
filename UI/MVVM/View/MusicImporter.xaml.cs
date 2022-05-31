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
    public partial class MusicImporter : UserControl
    {
        public MusicImporter()
        {
            InitializeComponent();
        }

        private void SelectMusicDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowDialog();

            var dir = dialog.SelectedPath;
            if(!Directory.Exists(dir))
            {
                ImportReport.Text = "Ist kein Ordner.";
                return;
            }

            var allFiles = SimpleDiskIndexer.Instance.Index(dir).ToList();
            int filesRegistered = 0;
            int filesTried = 0;
            foreach(var file in allFiles)
            {
                try
                {
                    filesTried++;
                    var music = Database.Instance.RegisterMusicSource(file);
                    filesRegistered++;
                } 
                catch (UnknownMusicFormat)
                {
                    continue;
                }
            }
            ImportReport.Text = $"{filesRegistered} von {filesTried} Musikdatei(n) registriert.";
        }

        private void ClearDatabase_Click(object sender, RoutedEventArgs e)
        {
            Database.Instance.ClearAll();
            ImportReport.Text = "Datenbank geleert.";
        }
    }
}
