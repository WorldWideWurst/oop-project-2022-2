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
    /// Bearbeitet von Philipp Funk und Janek Engel, nur temorär um Kompilierung zu ermöglichen
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            MessageBox.Show("Das funzt 1");
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    MessageBox.Show("Das funzt w");
                    LayoutRoot.Margin = new Thickness(8, 8, 8, 8);
                    break;
                case WindowState.Minimized:
                    MessageBox.Show("Das funzt 3");
                    LayoutRoot.Margin = new Thickness(0, 0, 0, 0);
                    break;
                case WindowState.Normal:
                    MessageBox.Show("Das funzt 4");
                    LayoutRoot.Margin = new Thickness(0, 0, 0, 0);
                    break;
            }
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

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Maximizing!");
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Liked the song!");
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                LayoutRoot.Margin = new Thickness(0, 0, 0, 0);
                this.WindowState = WindowState.Normal;
            }
            else
            {
                LayoutRoot.Margin = new Thickness(8, 8, 8, 8);
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
