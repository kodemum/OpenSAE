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
    /// Interaction logic for BackupHelperWindow.xaml
    /// </summary>
    public partial class BackupHelperWindow : Window
    {
        public BackupHelperWindow()
        {
            InitializeComponent();

            Width = Settings.Default.BackupManagerWindowWidth;
            Height = Settings.Default.BackupManagerWindowHeight;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.BackupManagerWindowWidth = Width;
            Settings.Default.BackupManagerWindowHeight = Height;
        }
    }
}
