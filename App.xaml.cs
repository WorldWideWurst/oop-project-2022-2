using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Project
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public string DefaultDownloadFolder
        {
            get
            {
                if(defaultDownloadFolder != null)
                {
                    return defaultDownloadFolder;
                }

                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.ShowDialog();

                defaultDownloadFolder = dialog.SelectedPath;
                return defaultDownloadFolder;
            }
        }
        string? defaultDownloadFolder;
        
    }
}
