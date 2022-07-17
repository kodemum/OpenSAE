using Microsoft.Win32;
using OpenSAE.Core;
using OpenSAE.Core.SAML;
using OpenSAE.Models;
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
        private SymbolArtModel? _symbol;
        private Dictionary<SymbolArtLayerModel, GeometryModel3D> _layerDictionary
            = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile(string filename)
        {
            try
            {
                using var fs = File.OpenRead(filename);

                _symbol = new SymbolArtModel(SamlLoader.LoadFromStream(fs));
                

                DataContext = _symbol;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Unable to open selected file: {ex.Message}");
                return;
            }

            RefreshCurrentSymbolArt();
        }

        private void RefreshCurrentSymbolArt()
        {
            model3dGroup.Children.Clear();
            model3dGroup.Children.Add(new AmbientLight(Colors.White));
            _layerDictionary.Clear();

            DrawSymbolArtGroup(_symbol);
        }

        private void DrawSymbolArtGroup(SymbolArtItemModel group)
        {
            for (int i = group.Children.Count - 1; i >= 0; i--)
            {
                if (group.Children[i] is SymbolArtLayerModel layer)
                {
                    Draw3d(layer);
                }
                else
                {
                    DrawSymbolArtGroup(group.Children[i]);
                }
            }

            group.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(SymbolArtItemModel.Visible))
                {
                    RecursiveRefreshVisibility(group);
                }
            };
        }

        private void RecursiveRefreshVisibility(SymbolArtItemModel group)
        {
            foreach (var item in group.Children)
            {
                if (item is SymbolArtLayerModel layer)
                {
                    RefreshLayerVisibility(layer);
                }
                else
                {
                    RecursiveRefreshVisibility(item);
                }
            }
        }

        private void Draw3d(SymbolArtLayerModel layer)
        {
            // GeometryModel3D does not support data binding :(

            // order:
            // left bottom
            // left top
            // right bottom
            // right top
            var points = new Point3DCollection(layer.Points);

            var uri = layer.SymbolPackUri;
            if (uri == null)
            {
                return;
            }

            var material = new DiffuseMaterial()
            {
                Brush = new ImageBrush((ImageSource)new ImageSourceConverter().ConvertFromString(uri)!)
                {
                    Opacity = layer.Alpha
                },
                AmbientColor = layer.Color
            };

            var model = new GeometryModel3D()
            {
                Geometry = new MeshGeometry3D()
                {
                    Positions = points,
                    TextureCoordinates = new PointCollection()
                    {
                        new Point(0, 1),
                        new Point(0, 0),
                        new Point(1, 1),
                        new Point(1, 0)
                    },
                    TriangleIndices = new Int32Collection()
                    {
                        0, 2, 1,
                        2, 3, 1,
                    }
                },
                Material = material,
                BackMaterial = material,
            };

            model3dGroup.Children.Add(model);

            _layerDictionary.Add(layer, model);

            layer.PropertyChanged += (_, __) =>
            {
                RefreshLayerVisibility(layer);
            };
        }

        private void RefreshLayerVisibility(SymbolArtLayerModel layer)
        {
            var model3d = _layerDictionary[layer];

            ((DiffuseMaterial)model3d.Material).Brush.Opacity = layer.IsVisible ? layer.Alpha : 0;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog()
            {
                Filter = "SAML symbol art (*.saml)|*.saml",
                Title = "Open existing symbol art file"
            };

            if (od.ShowDialog() == true)
            {
                OpenFile(od.FileName);
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_symbol != null)
            {
                _symbol.SelectedItem = (SymbolArtItemModel)e.NewValue;
            }
        }
    }
}
