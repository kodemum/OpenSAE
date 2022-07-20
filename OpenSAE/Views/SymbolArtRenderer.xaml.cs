﻿using OpenSAE.Core;
using OpenSAE.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static readonly DependencyProperty SelectedLayerProperty =
            DependencyProperty.Register(
              name: "SelectedLayer",
              propertyType: typeof(SymbolArtItemModel),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata(
                  defaultValue: null,
                  flags: FrameworkPropertyMetadataOptions.AffectsRender
                  )
        );

        public static readonly DependencyProperty MouseSymbolPositionProperty =
            DependencyProperty.Register(
              name: "MouseSymbolPosition",
              propertyType: typeof(Point),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata()
        );

        /// <summary>
        /// The radius of a layer vertex (in symbol art units) where a click will be considered
        /// to have hit that vertex.
        /// </summary>
        private const int LayerVertexClickRadius = 4;

        private Dictionary<SymbolArtLayerModel, LayerModelReference> _layerDictionary
            = new();

        public SymbolArtRenderer()
        {
            InitializeComponent();
        }

        private bool isDragging;
        private Point rotatingOrigin;
        private bool isRotating;
        private int draggingVertexIndex;
        private Point draggingClickOrigin;
        private Point draggingLayerOriginalPos;

        public Point MouseSymbolPosition
        {
            get => (Point)GetValue(MouseSymbolPositionProperty);
            set => SetValue(MouseSymbolPositionProperty, value);
        }

        public ISymbolArtItem SelectedLayer
        {
            get => (ISymbolArtItem)GetValue(SelectedLayerProperty);
            set => SetValue(SelectedLayerProperty, value);
        }

        /// <summary>
        /// Converts the coordinate system from relative to the view to the one used in the symbol art
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private Point CoordinatesToSymbolArt(Point target)
        {
            // we have (arbitrarily) assigned the width of the renderer to be 240 symbol art units
            // (due to wanting to display some space around the symbol art)
            var factor = viewport3d.ActualWidth / 240;

            // additionally the coordinate system for the symbol arts starts with 0,0 at the center
            // so we need to subtract half of the width/height of the view
            return new Point(
                (target.X - viewport3d.ActualWidth / 2) / factor,
                (target.Y - viewport3d.ActualHeight / 2) / factor
                );
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Cursor = Cursors.Hand;
            }

            if (SelectedLayer != null)
            {
                switch (e.Key)
                {
                    case Key.Down:
                        SelectedLayer.Position += new Vector(0, 1);
                        e.Handled = true;
                        break;

                    case Key.Up:
                        SelectedLayer.Position += new Vector(0, -1);
                        e.Handled = true;
                        break;

                    case Key.Left:
                        SelectedLayer.Position += new Vector(-1, 0);
                        e.Handled = true;
                        break;

                    case Key.Right:
                        SelectedLayer.Position += new Vector(1, 0);
                        e.Handled = true;
                        break;
                }
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            Cursor = null;

            base.OnPreviewKeyUp(e);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs args)
        {
            Focus();
            Keyboard.Focus(this);

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                // selection mode
                HitTestResult result = VisualTreeHelper.HitTest(viewport3d, args.GetPosition(viewport3d));

                if (result is RayMeshGeometry3DHitTestResult meshHit)
                {
                    var hitLayer = _layerDictionary.FirstOrDefault(x => x.Value.Geometry == meshHit.MeshHit);

                    if (hitLayer.Key != null)
                    {
                        SelectedLayer = hitLayer.Key;
                    }
                }

                return;
            }

            // if no layer is selected, there's nothing to manipulate
            if (SelectedLayer is not ISymbolArtItem layer)
                return;

            // get the mouse location convert the coordinates
            draggingClickOrigin = CoordinatesToSymbolArt(args.GetPosition(viewport3d));

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                isRotating = true;
                rotatingOrigin = layer.Vertices.GetCenter();
            }
            else
            {
                // get the distance from the mouse location to each vertex of the target layer
                var distanceToVertices = layer.Vertices
                    .Select(point => (point - draggingClickOrigin).Length)
                    .ToArray();

                // find the closest vertex
                draggingVertexIndex = distanceToVertices.GetMinIndexBy(x => x);

                // check if the closest vertex is close enough that we'll consider that it was clicked
                if (distanceToVertices[draggingVertexIndex] > LayerVertexClickRadius)
                {
                    // drag entire symbol if we're not close enough to any vertex
                    draggingVertexIndex = -1;
                    draggingLayerOriginalPos = layer.Position;
                }

                isDragging = true;
            }

            CaptureMouse();
            args.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs args)
        {
            base.OnMouseMove(args);

            Point ptMouse = CoordinatesToSymbolArt(args.GetPosition(viewport3d));

            MouseSymbolPosition = new Point(Math.Round(ptMouse.X), Math.Round(ptMouse.Y));

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                // highlight layers under cursor
                HitTestResult result = VisualTreeHelper.HitTest(viewport3d, args.GetPosition(viewport3d));

                if (result is RayMeshGeometry3DHitTestResult meshHit)
                {
                    var hitLayer = _layerDictionary.FirstOrDefault(x => x.Value.Geometry == meshHit.MeshHit);

                    if (hitLayer.Key != null)
                    {
                        SelectedLayer = hitLayer.Key;
                        return;
                    }
                }
            }

            if (SelectedLayer == null)
                return;

            if (isRotating)
            {
                var now = ptMouse;

                var angleBefore = Math.Atan2(draggingClickOrigin.Y - rotatingOrigin.Y, draggingClickOrigin.X - rotatingOrigin.X) * 180 / Math.PI;
                var angleNow = Math.Atan2(now.Y - rotatingOrigin.Y, now.X - rotatingOrigin.X) * 180 / Math.PI;
                var angle = angleNow - angleBefore;

                SelectedLayer.TemporaryRotate(angle);
            }
            else if (isDragging)
            {
                if (draggingVertexIndex == -1)
                {
                    var diff = ptMouse - draggingClickOrigin;

                    SelectedLayer.Position = draggingLayerOriginalPos + new Vector(Math.Round(diff.X), Math.Round(diff.Y));
                }
                else
                {
                    SelectedLayer.SetVertex(draggingVertexIndex, ptMouse);
                }

                args.Handled = true;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs args)
        {
            base.OnMouseUp(args);

            if (isRotating)
            {
                SelectedLayer.CommitRotate();
            }

            if (isDragging || isRotating)
            {
                isDragging = false;
                isRotating = false;
                ReleaseMouseCapture();
                args.Handled = true;
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Redraw();
        }

        private void Redraw()
        {
            symbolArtContentGroup.Children.Clear();
            symbolArtContentGroup.Children.Add(new AmbientLight(Colors.White));
            _layerDictionary.Clear();

            if (DataContext is SymbolArtModel sa)
            {
                DrawSymbolArtGroup(sa);
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

            group.Children.CollectionChanged += Children_CollectionChanged;
        }

        private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (SymbolArtItemModel deletedItem in e.OldItems!)
                    {
                        foreach (var deletedLayer in deletedItem.GetAllLayers())
                        {
                            if (_layerDictionary.TryGetValue(deletedLayer, out LayerModelReference? refs))
                            {
                                symbolArtContentGroup.Children.Remove(refs.Model);
                                _layerDictionary.Remove(deletedLayer);
                            }
                        }
                    }
                    break;

                default:
                    // lets just redraw everything - traversing the entire tree to 
                    // figure out the order the elements should be in can wait
                    Redraw();
                    break;
            }
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

            var brush = new ImageBrush(layer.Symbol?.Image)
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
                    refs.Brush.ImageSource = layer.Symbol?.Image;
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

        private void renderer_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);
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
