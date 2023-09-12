using OpenSAE.Core;
using OpenSAE.Models;
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
    /// Interaction logic for BitmapConverterWindow.xaml
    /// </summary>
    public partial class BitmapConverterWindow : Window
    {
        private readonly BitmapConverterModel _model;

        public BitmapConverterWindow(BitmapConverterModel model)
        {
            InitializeComponent();

            _model = model;

            DataContext = _model;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (e.Data is IDataObject dataObject)
            {
                var files = (string[]?)dataObject.GetData(DataFormats.FileDrop);

                if (files?.Length == 1)
                {
                    e.Effects |= DragDropEffects.Copy;
                    e.Handled = true;
                }
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data is IDataObject dataObject)
            {
                var files = (string[]?)dataObject.GetData(DataFormats.FileDrop);

                if (files?.Length == 1)
                {
                    _model.BitmapFilename = files[0];
                    e.Handled = true;
                }
            }
        }

        private void ListBoxScroll_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _model.SymbolCommand.Execute("add");
        }
    }
}
