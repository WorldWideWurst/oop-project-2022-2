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
using Project;

namespace Project.UI
{
    /// Bearbeitet von Philipp Funk und Janek Engel
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("'Never gonna give you up' - Rick Astley");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Went to last song");
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Skipped song");
        }

        private void RandomizeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Randomizing Song Order!");
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Repeating this song!");
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Maximizing!");
        }
        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Liked the song!");
        }
    }
}
