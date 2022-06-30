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
using Project.Data;

namespace Project.UI.MVVM.View.LibraryPages
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class SourceOverview : UserControl, ILibraryPage
    {
        public SourceOverview(Source source)
        {
            InitializeComponent();
            DataContext = new SourceViewModel(source);
        }

        public void Overview()
        {
            throw new NotImplementedException();
        }

        public void Search(string queryString)
        {
            throw new NotImplementedException();
        }

        private void ViewSourceInExplorer(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hier öffnet sich nun der Explorer TODO");
        }

        private void ViewMusic(object sender, RoutedEventArgs e)
        {
            var music = ((SourceViewModel)DataContext).Source.Music.Target;
            var ctrl = new MusicOverview(music);
            ((MainWindow)Application.Current.MainWindow).LibraryTab.ShowLibraryPage(ctrl);
        }
    }

    class SourceViewModel
    {
        public readonly Source Source;

        public string Address => Source.Address;

        public string MusicDescription
        {
            get
            {
                var music = Database.Instance.GetMusic(Source.MusicId);
                var sb = new StringBuilder();
                sb.AppendJoin(", ", music.Artists.Target.Select(mba => mba.ArtistId));
                sb.Append(sb.Length > 0? " - " : "").Append(music.Title ?? "");
                return sb.Length > 0 ? sb.ToString() : Address;
            }
        }

        public string Type => Source.SourceType switch
        {
            SourceType.Local => "Lokal",
            SourceType.Stream => "Stream",
        };

        public SourceViewModel(Source source)
        {
            this.Source = source;
        }
    }
}
