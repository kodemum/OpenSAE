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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenSAE.Views
{
    /// <summary>
    /// Interaction logic for SymbolSelector.xaml
    /// </summary>
    public partial class SymbolSelector : UserControl
    {
        public static readonly DependencyProperty SelectedSymbolProperty =
            DependencyProperty.Register(
              name: "SelectedSymbol",
              propertyType: typeof(Symbol),
              ownerType: typeof(SymbolSelector),
              typeMetadata: new FrameworkPropertyMetadata(
                  defaultValue: null,
                  flags: FrameworkPropertyMetadataOptions.AffectsRender
                  )
        );

        public SymbolSelector()
        {
            InitializeComponent();
        }

        public Symbol? SelectedSymbol
        {
            get => (Symbol)GetValue(SelectedSymbolProperty);
            set => SetValue(SelectedSymbolProperty, value);
        }
    }
}
