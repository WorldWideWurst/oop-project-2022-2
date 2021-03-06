// verfasst von Janek Engel und Philipp Funk <3

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

// erstellt von Janek Engel, haupsächliche Design und Implementierungsarbeit von Phil Funk

namespace Project.UI.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für Fullscreen.xaml
    /// </summary>
    public partial class Fullscreen : UserControl
    {
        public Fullscreen()
        {
            InitializeComponent();
            Player.Player.Instance.CurrentMusicChanged += Player_MusicChanged;
        }

        private void Player_MusicChanged(Data.Music music, int index)
        {
            FullscreenTitle.Text = music.Title;
        }
    }
}
