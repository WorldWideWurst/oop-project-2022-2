using Project.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Project.UI.MVVM.View.LibraryPages
{
    /// <summary>
    /// Interaktionslogik für Main.xaml
    /// </summary>
    public partial class Main : UserControl, ILibraryPage
    {

        public ObservableCollection<IMusicList> LibraryEntries { get; } = Player.Library.Instance.Entries;

        public Main()
        {
            InitializeComponent();
            Data.Database.Instance.DatabaseChanged += (type, _) => 
            { 
                if (type == typeof(MusicInList) || type == typeof(MusicList))
                {
                    Application.Current.Dispatcher.Invoke(() => 
                    {
                        Player.Library.Instance.Refresh();
                        Items.Items.Refresh();
                    }); 
                } 
            };
            DataContext = this;
        }


        public void Search(string queryString)
        {
            throw new NotImplementedException();
        }

        public void Overview()
        {
            throw new NotImplementedException();
        }

    }
}
