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
        private Dictionary<SymbolArtLayerModel, LayerModelReference> _layerDictionary
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
            var points = new Point3DCollection(layer.Points3D);

            var uri = layer.SymbolPackUri;
            if (uri == null)
            {
                return;
            }

            var brush = new ImageBrush((ImageSource)new ImageSourceConverter().ConvertFromString(uri)!)
            {
                Opacity = layer.Alpha
            };

            var material = new DiffuseMaterial()
            {
                Brush = brush,
                AmbientColor = layer.Color
            };

            var geometry = new MeshGeometry3D()
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
            };

            var model = new GeometryModel3D()
            {
                Geometry = geometry,
                Material = material,
                BackMaterial = material,
            };

            symbolArtContentGroup.Children.Add(model);

            _layerDictionary.Add(layer, new LayerModelReference(model, material, brush, geometry));

            layer.PropertyChanged += Layer_PropertyChanged;
        }

        private void Layer_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not SymbolArtLayerModel layer)
                return;

            var refs = _layerDictionary[layer];

            switch (e.PropertyName)
            {
                case nameof(layer.Visible):
                case nameof(layer.Alpha):
                    RefreshLayerVisibility(layer);
                    break;

                case nameof(layer.Symbol):
                    refs.Brush.ImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString(layer.SymbolPackUri)!;
                    break;

                case nameof(layer.Color):
                    refs.Material.AmbientColor = layer.Color;
                    break;

                default:
                    if (e.PropertyName?.StartsWith("Vertex") == true)
                    {
                        refs.Geometry.Positions = new Point3DCollection(layer.Points3D);
                    }
                    break;
            }
        }

        private void RefreshLayerVisibility(SymbolArtLayerModel layer)
        {
            if (!_layerDictionary.TryGetValue(layer, out var refs))
            {
                // we may encounter layers not in the dictionary if they were
                // skipped because of an unknown symbol
                return;
            }

            refs.Brush.Opacity = layer.IsVisible ? layer.Alpha : 0;
        }

        /// <summary>
        /// Contains view references for a layer model
        /// </summary>
        private class LayerModelReference
        {
            public LayerModelReference(GeometryModel3D model, DiffuseMaterial material, ImageBrush brush, MeshGeometry3D geometry)
            {
                Model = model;
                Material = material;
                Brush = brush;
                Geometry = geometry;
            }

            public GeometryModel3D Model { get; }

            public DiffuseMaterial Material { get; }

            public ImageBrush Brush { get; }

            public MeshGeometry3D Geometry { get; }
        }
    }
}
