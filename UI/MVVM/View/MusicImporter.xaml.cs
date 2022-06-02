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
using System.Threading;

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
            SuccessfulListExpander.Header = 0;
            ErrorListExpander.Header = 0;
            ConvertedListExpander.Header = 0;
            AlreadyExistsListExpander.Header = 0;
        }

        record struct ImportReportEntry(string File, string? ToolTip = null);
        record ReportListHeader(string Name, int Count);

        private async void SelectMusicDir_Click(object sender, RoutedEventArgs e)
        {
            // Todo: das hier irgendwie auslagern
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

            var beginTime = DateTime.Now;
            int filesTried = 0;
            foreach (var file in SimpleDiskIndexer.Instance.Index(dir))
            {
                Task.Run(() => 
                {
                    ImportFile(file, dir);
                    Interlocked.Increment(ref filesTried);
                });
            }

            MessageBox.Show($"Import von {filesTried} Datei(n) abgeschlossen.\r\nBenötigte Zeit: {(beginTime - DateTime.Now)}");
        }

        async void ImportFile(string file, string dir)
        {
            string? failureReason = null;

            try
            {
                // existiert die datei schon in der DAtenbank?
                Source? source = Database.Instance.GetSource(file);
                if (source != null)
                {
                    AddToList(AlreadyExistsList, AlreadyExistsListExpander, new ImportReportEntry(file[(dir.Length + 1)..]));
                    return;
                }

                // Musik laden. Kann fehlschlagen
                MusicFileMeta meta = MetaLoader.Instance.Load(file);
                Music music = Database.Instance.RegisterMusicSource(meta);

                // erfolg
                AddToList(SuccessfulList, SuccessfulListExpander, new ImportReportEntry(file[(dir.Length + 1)..]));
            }
            catch (Exception ex) when (
                ex is UnknownMusicFileFormat ||
                ex is FileNotFoundException ||
                ex is InvalidMusicFileFormat)
            {
                // wenns an unbekannten Dateiformaten liegt, versuchen zu konvertieren
                if (ex is UnknownMusicFileFormat)
                {
                    string? targetFile = System.IO.Path.ChangeExtension(file, ".mp3");
                    if (targetFile == null)
                        return;

                    try
                    {
                        AddToList(ConvertedList, ConvertedListExpander, new ImportReportEntry(file[(dir.Length + 1)..]));
                        return;
                    }
                    catch (Convert.UnsupportedFileConversion)
                    {
                        // kann nicht in angegebenes Format konvertieren
                        failureReason = $"Unbekanntes Dateiformat '{System.IO.Path.GetExtension(file)}'. Konnte weder eingelesen noch konvertiert werden.";
                    }
                    catch (Convert.ConversionException)
                    {
                        // konvertierung in angegebenes Format nicht möglich
                        failureReason = $"Konnte nicht eingelesen werden und beim Konvertieren trat ein Fehler auf.";
                    }
                    catch (NotImplementedException)
                    {
                        // Komm Ben, Programmier mal was
                        failureReason = $"Ben hat hier noch nichts implementiert. Frech.";
                    }
                }
                else if (ex is FileNotFoundException)
                {
                    failureReason = "Diese Datei konnte nicht gefunden werden.";
                }
                else if (ex is InvalidMusicFileFormat)
                {
                    failureReason = "Es gab Probleme beim Einlesen der Datei.";
                }

                ErrorList.Items.Add(new ImportReportEntry(file[(dir.Length + 1)..], failureReason));
                return;
            }
        }

        void ClearLists()
        {
            SuccessfulList.Items.Clear();
            ErrorList.Items.Clear();
            AlreadyExistsList.Items.Clear();
        }

        void AddToList(ListView list, Expander expander, ImportReportEntry entry)
        {
            lock(this)
            {
                list.Items.Add(entry);
                expander.Header = ((int)expander.Header) + 1;
            }
        }

        void AlreadyExistsExpander_Expand(object sender, RoutedEventArgs e)
        {

        }
    }
}
