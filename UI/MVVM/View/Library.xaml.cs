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

namespace Project.UI.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für Bibliothek.xaml
    /// </summary>
    public partial class Library : UserControl
    {

        readonly IList<ILibraryPage> pageHistory = new List<ILibraryPage>();

        public Library()
        {
            InitializeComponent();
        }

        public void ShowLibraryPage(ILibraryPage? page)
        {
            if(page == null)
            {
                if(pageHistory.Count > 0)
                {
                    LibraryContent.Content = pageHistory[0];
                    pageHistory.Clear();
                    UpdateGoBackVisibility();
                }
            }
            else
            {
                pageHistory.Add((ILibraryPage)LibraryContent.Content);
                LibraryContent.Content = page;
                UpdateGoBackVisibility();
            }
        }

        private void ShowPreviousPage()
        {
            if(pageHistory.Count > 0)
            {
                LibraryContent.Content = pageHistory.Last();
                pageHistory.RemoveAt(pageHistory.Count - 1);
            }
            UpdateGoBackVisibility();
        }

        private void UpdateGoBackVisibility()
        {
            GoBackButton.Visibility = pageHistory.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPreviousPage();
        }
    }
}
