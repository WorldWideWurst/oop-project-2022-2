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

//erstellt von Richard Förster

namespace Project.UI.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für ImportLogHeader.xaml
    /// </summary>
    public partial class ImportLogHeader : UserControl
    {

        public string HeaderText { get; set; } = "";

        public ImportLogHeader()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
