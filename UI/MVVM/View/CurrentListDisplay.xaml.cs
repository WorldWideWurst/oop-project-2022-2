﻿using System;
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
	/// Interaktionslogik für CurrentListDisplay.xaml
	/// </summary>
	public partial class CurrentListDisplay : UserControl
    {
        public CurrentListDisplay()
        {
            InitializeComponent();
            DataContext = Player.Player.Instance.CurrentList;

            Player.Player.Instance.CurrentMusicChanged += (music, index) =>
            {
                MessageBox.Show("Scheiß Microsoft");
            };
        }
    }
}
