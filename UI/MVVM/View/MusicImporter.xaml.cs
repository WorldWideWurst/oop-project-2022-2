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
            if(dir == string.Empty)
            {
                // abgebrochen
                return;
            }

            if(!Directory.Exists(dir))
            {
                MessageBox.Show("Dieser Ordner existiert nicht.");
                return;
            }

            ClearLists();

            int filesAdded = 0;
            int filesDiscarded = 0;
            int filesTried = 0;
            foreach(var file in SimpleDiskIndexer.Instance.Index(dir))
            {
                try
                {
                    filesTried++;

                    // existiert die datei schon in der DAtenbank?
                    Source? source = Database.Instance.GetSource(file);
                    if(source != null)
                    {
                        AlreadyExistsList.Items.Add(new ListViewItem()
                        {
                            Content = file[(dir.Length + 1)..]
                        });
                        continue;
                    }

                    // Musik laden. Kann fehlschlagen
                    MusicFileMeta meta = MetaLoader.Instance.Load(file);
                    Music music = Database.Instance.RegisterMusicSource(meta);

                    // erfolg
                    filesAdded++;
                    SuccessfulList.Items.Add(new ListViewItem()
                    {
                        Content = file[(dir.Length + 1)..]
                    });
                } 
                catch (Exception ex) when (
                    ex is UnknownMusicFormat ||
                    ex is FileNotFoundException ||
                    ex is InvalidMusicFileFormat)
                {
                    filesDiscarded++;
                    ErrorList.Items.Add(new ListViewItem()
                    {
                        ToolTip = ex.Message,
                        Content = file[(dir.Length + 1)..]
                    });
                    continue;
                }
            }

            ErrorListExpander.IsExpanded = true;
            MessageBox.Show($@"{filesAdded} Datei(n) hinzugefügt, {filesDiscarded} verworfen, {filesTried - filesAdded - filesDiscarded} ignoriert.
Insgesamt {filesTried} Datei(n)");
        }

        private void ClearDatabase_Click(object sender, RoutedEventArgs e)
        {
            Database.Instance.ClearAll();
            ClearLists();
            MessageBox.Show("Datenbank geleert.");
        }

        void ClearLists()
        {
            SuccessfulList.Items.Clear();
            ErrorList.Items.Clear();
            AlreadyExistsList.Items.Clear();
        }

        void AlreadyExistsExpander_Expand(object sender, RoutedEventArgs e)
        {

        }
    }
}
