using Microsoft.Win32;
using OpenSAE.Core;
using OpenSAE.Core.SAML;
using OpenSAE.Models;
using OpenSAE.Services;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenSAE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AppModel _model;

        public MainWindow()
        {
            InitializeComponent();

            _model = new AppModel(new DialogService(this));
            _model.ExitRequested += (_, __) => Close();

            DataContext = _model;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_model != null)
            {
                _model.SelectedItem = (SymbolArtItemModel)e.NewValue;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_model.RequestExit())
            {
                e.Cancel = true;
            }
        }
    }
}
