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

//erstellt von Richard Förster

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

        record struct ImportReportEntry(string File, string? ToolTip = null);
        record ReportListHeader(string Name, int Count);

        private void SelectMusicDir_Click(object sender, RoutedEventArgs e)
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

            SelectMusicDir.IsEnabled = false;
            SelectMusicDir.Content = String.Format((string)Resources["SelectMusicDir_Busy"], System.IO.Path.GetDirectoryName(dir) ?? dir);

            var worker = new System.ComponentModel.BackgroundWorker();
            var filesTried = 0;
            var beginTime = DateTime.Now;

            worker.DoWork += (sender, args) =>
            {
                foreach (var file in SimpleDiskIndexer.Instance.Index(dir))
                {
                    Application.Current.Dispatcher.Invoke(ImportFile(file, dir));
                    filesTried++;
                }
            };
            worker.RunWorkerCompleted += (sender, args) =>
            {
                MessageBox.Show(
$@"Import von {filesTried} Datei(n) abgeschlossen.
Benötigte Zeit: {(DateTime.Now - beginTime)}");
                SelectMusicDir.IsEnabled = true;
                SelectMusicDir.Content = (string)Resources["SelectMusicDir_Idle"];
            };

            worker.RunWorkerAsync();

        }

        Action ImportFile(string file, string dir)
        {
            string? failureReason = null;

            try
            {
                // existiert die datei schon in der DAtenbank?
                Source? source = Database.Instance.GetSource(file);
                if (source != null)
                {
                    
                    return () => AddToList(AlreadyExistsList, AlreadyExistsListExpander, new ImportReportEntry(file[(dir.Length + 1)..]));
                    
                }

                // Musik laden. Kann fehlschlagen
                MusicFileMeta meta = MetaLoader.Instance.Load(file);
                Music music = Database.Instance.RegisterMusicSource(meta);

                // erfolg
                return () => AddToList(SuccessfulList, SuccessfulListExpander, new ImportReportEntry(file[(dir.Length + 1)..]));
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
                        return () => { };

                    if(File.Exists(targetFile))
                    {
                        return () => AddToList(AlreadyExistsList, AlreadyExistsListExpander, new ImportReportEntry(file[(dir.Length + 1)..]));
                    }

                    try
                    {
                        Convert.Converter.Instance.Convert(file, targetFile);
                        return () => AddToList(ConvertedList, ConvertedListExpander, new ImportReportEntry(file[(dir.Length + 1)..]));
                        
                    }
                    catch (Convert.UnsupportedFileConversion)
                    {
                        // kann nicht in angegebenes Format konvertieren
                        failureReason = $"Unbekanntes Dateiformat '{System.IO.Path.GetExtension(file)}'. Konnte weder eingelesen noch konvertiert werden.";
                    }
                    catch (Convert.ConversionException)
                    {
                        // konvertierung in angegebenes Format nicht möglich
                        failureReason = $"Datei konnte nicht eingelesen werden und beim Konvertieren trat ein Fehler auf.";
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

                return () => AddToList(ErrorList, ErrorListExpander, new ImportReportEntry(file[(dir.Length + 1)..], failureReason));
            }
            
        }

        void AddToList(ListView list, Expander expander, ImportReportEntry entry)
        {
            list.Items.Add(entry);
            var header = (ImportLogHeader)expander.Header;
            header.Counter.Text = list.Items.Count.ToString();
        }

        void AlreadyExistsExpander_Expand(object sender, RoutedEventArgs e)
        {

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
