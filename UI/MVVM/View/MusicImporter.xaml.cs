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

            ClearLists();

            int filesAdded = 0;
            int filesDiscarded = 0;
            int filesTried = 0;
            int filesConverted = 0;
            string? failureReason = null;
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
                    ex is UnknownMusicFileFormat ||
                    ex is FileNotFoundException ||
                    ex is InvalidMusicFileFormat)
                {
                    // wenns an unbekannten Dateiformaten liegt, versuchen zu konvertieren
                    if(ex is UnknownMusicFileFormat)
                    {
                        string? targetFile = System.IO.Path.ChangeExtension(file, ".mp3");
                        if (targetFile == null)
                            continue;

                        try
                        {
                            Convert.Converter.Instance.Convert(file, targetFile);
                            ConvertedList.Items.Add(new ListViewItem()
                            {   
                                ToolTip = $"Konvertiert von {file[(dir.Length + 1)..]}",
                                Content = targetFile[(dir.Length + 1)..].ToString()
                            });
                            filesAdded++;
                            filesConverted++;
                            continue;
                        }
                        catch(Convert.UnsupportedFileConversion ucEx)
                        {
                            // kann nicht in angegebenes Format konvertieren
                            failureReason = $"Unbekanntes Dateiformat '{System.IO.Path.GetExtension(file)}'. Konnte weder eingelesen noch konvertiert werden.";
                        }
                        catch(Convert.ConversionException cEx)
                        {
                            // konvertierung in angegebenes Format nicht möglich
                            failureReason = $"Konnte nicht eingelesen werden und beim Konvertieren trat ein Fehler auf.";
                        }
                        catch(NotImplementedException niEx)
                        {
                            // Komm Ben, Programmier mal was
                            failureReason = $"Ben hat hier noch nichts implementiert. Frech.";
                        }
                    }
                    else if(ex is FileNotFoundException)
                    {
                        failureReason = "Diese Datei konnte nicht gefunden werden.";
                    }
                    else if(ex is InvalidMusicFileFormat)
                    {
                        failureReason = "Es gab Probleme beim Einlesen der Datei.";
                    }

                    filesDiscarded++;
                    ErrorList.Items.Add(new ListViewItem()
                    {
                        ToolTip = failureReason,
                        Content = file[(dir.Length + 1)..]
                    });
                    continue;
                }
            }

            ErrorListExpander.IsExpanded = true;
            MessageBox.Show(
$@"Insgesamt {filesTried} Datei(n) verarbeitet:
{filesAdded} Datei(n) hinzugefügt,
{filesConverted} musste(n) konvertiert werden.
{filesDiscarded} Datei(n) nicht importiert,
{filesTried - filesAdded - filesDiscarded} schon vorhanden.
");
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
