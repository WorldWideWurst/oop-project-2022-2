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
using Project.Player;

namespace Project.UI.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für Einstellungen.xaml
    /// </summary>

    public partial class Settings : UserControl
    {

        public Settings()
        {
            InitializeComponent();
        }

        private void ClearDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Willst du die Datenback leeren?" + Environment.NewLine + Environment.NewLine + "Dabei werden NICHT die Dateien auf deinem Rechner, sondern nur deren Eintrag in diesem Programm entfernt!", "Datenback leeren", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Database.Instance.ClearAll();
                    MessageBox.Show("Datenbank wurde geleert.");
                    break;
                case MessageBoxResult.No:
                    break;
            }

        }

        private void TickpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
                Player.Player.Instance.Tickspeed = TimeSpan.FromSeconds(e.NewValue);
        }
    }
}
