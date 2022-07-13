using Project.UI.MVVM.View.LibraryPages;
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

// verfasst von Richard Förster

namespace Project.UI.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für Song.xaml
    /// </summary>
    public partial class MusicOverview : UserControl, ILibraryPage
    {
        public MusicOverview(Data.Music music)
        {
            InitializeComponent();

            DataContext = music;
        }

        public void Overview()
        {
            throw new NotImplementedException();
        }

        public void Search(string queryString)
        {
            throw new NotImplementedException();
        }

        private void ViewSource_Click(object sender, RoutedEventArgs e)
        {
            var ctrl = new SourceOverview((Data.Source)(((Button)sender).DataContext));
            ((MainWindow)Application.Current.MainWindow).LibraryTab.ShowLibraryPage(ctrl);
        }
    }
}
