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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenSAE.Views
{
    /// <summary>
    /// Interaction logic for SymbolArtRenderer.xaml
    /// </summary>
    public partial class SymbolArtRenderer : UserControl
    {
        private Dictionary<SymbolArtLayerModel, GeometryModel3D> _layerDictionary
            = new();

        public SymbolArtRenderer()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            symbolArtContentGroup.Children.Clear();
            symbolArtContentGroup.Children.Add(new AmbientLight(Colors.White));
            _layerDictionary.Clear();

            if (e.NewValue is null)
            {
                // we've cleared the data, nothing more to do
                return;
            }
            else if (e.NewValue is SymbolArtModel sa)
            {
                DrawSymbolArtGroup(sa);
            }
            else
            {
                throw new InvalidOperationException("DataContext for SymbolArtRenderer must be of type SymbolArtModel");
            }
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

            symbolArtContentGroup.Children.Add(model);

            _layerDictionary.Add(layer, model);

            layer.PropertyChanged += Layer_PropertyChanged;
        }

        private void Layer_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not SymbolArtLayerModel layer)
                return;

            var model3d = _layerDictionary[layer];

            switch (e.PropertyName)
            {
                case nameof(layer.Visible):
                case nameof(layer.Alpha):
                    RefreshLayerVisibility(layer);
                    break;

                case nameof(layer.Symbol):
                    ((ImageBrush)((DiffuseMaterial)model3d.Material).Brush).ImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString(layer.SymbolPackUri)!;
                    break;

                case nameof(layer.Color):
                    ((DiffuseMaterial)model3d.Material).AmbientColor = layer.Color;
                    break;
            }
        }

        private void RefreshLayerVisibility(SymbolArtLayerModel layer)
        {
            if (!_layerDictionary.TryGetValue(layer, out var model3d))
            {
                // we may encounter layers not in the dictionary if they were
                // skipped because of an unknown symbol
                return;
            }

            ((DiffuseMaterial)model3d.Material).Brush.Opacity = layer.IsVisible ? layer.Alpha : 0;
        }
    }
}
