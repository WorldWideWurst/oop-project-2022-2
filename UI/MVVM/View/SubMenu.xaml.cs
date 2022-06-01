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



namespace Project.UI.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für SubMenu.xaml
    /// </summary>
    public partial class SubMenu : UserControl
    {
        public SubMenu()
        {
            //Initialisiert und zeigt PlaylistDisplay an
            DataContext = new PlaylistDisplay();
            InitializeComponent();
        }


        private void CurrentListDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new CurrentListDisplay();
        }

        private void PlaylistDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new PlaylistDisplay();
        }
    }
}
