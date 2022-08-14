using OpenSAE.Properties;
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
using System.Windows.Shapes;

namespace OpenSAE
{
    /// <summary>
    /// Interaction logic for BrowseFilesWindow.xaml
    /// </summary>
    public partial class BrowseFilesWindow : Window
    {
        public BrowseFilesWindow()
        {
            InitializeComponent();

            Width = Settings.Default.BrowseWindowWidth;
            Height = Settings.Default.BrowseWindowHeight;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.BrowseWindowWidth = Width;
            Settings.Default.BrowseWindowHeight = Height;
        }
    }
}
