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
using Project.Data;

namespace Project.UI.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für Startseite.xaml
    /// </summary>
    public partial class Startseite : UserControl
    {

        private MediaPlayer mediaPlayer = new MediaPlayer();

        public Startseite()
        {
            InitializeComponent();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var testFile = "C:\\Users\\Foerster\\Downloads\\track7AM0vkVSjpK9aNYwjNBc_Abèh & Cirox - B2 Pellegrino (Master).mp3";
            var music = MusicLoader.Instance.Load(testFile);
        }
    }
}
