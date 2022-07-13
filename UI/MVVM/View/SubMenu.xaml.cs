using Project.UI.MVVM.ViewModel;
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

//erstellt von Janek Engel

namespace Project.UI.MVVM.View
{
    // Verfasst von Janek Engel
    public partial class SubMenu : UserControl
    {

        private readonly CurrentListDisplay CurrentListDisplay = new();
        private readonly PlaylistDisplay PlaylistDisplay = new();

        public SubMenu()
        {
            //Initialisiert und zeigt PlaylistDisplay an
            InitializeComponent();
            DataContext = PlaylistDisplay;
        }


        private void CurrentListDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = CurrentListDisplay;
        }

        private void PlaylistDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = PlaylistDisplay;
        }
    }
}
