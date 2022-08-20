using OpenSAE.Core;
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
    public partial class SymbolArtRenderer : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SymbolArtProperty =
            DependencyProperty.Register(
              name: "SymbolArt",
              propertyType: typeof(SymbolArtModel),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata(
                  defaultValue: null,
                  flags: FrameworkPropertyMetadataOptions.AffectsRender,
                  PropertyChangedRedrawNecessary
                  )
        );

        public static readonly DependencyProperty ViewPositionProperty =
            DependencyProperty.Register(
              name: "ViewPosition",
              propertyType: typeof(Point),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata(
                  defaultValue: new Point(0, 0),
                  flags: FrameworkPropertyMetadataOptions.AffectsRender
                  )
        );

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

        public static readonly DependencyProperty ApplyToneCurveProperty =
            DependencyProperty.Register(
              name: "ApplyToneCurve",
              propertyType: typeof(bool),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata(defaultValue: false, PropertyChangedRedrawNecessary)
        );

        public static readonly DependencyProperty ShowGuidesProperty =
            DependencyProperty.Register(
              name: "ShowGuides",
              propertyType: typeof(bool),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata(defaultValue: false)
        );

        public static readonly DependencyProperty ShowBoundingBoxProperty =
            DependencyProperty.Register(
              name: "ShowBoundingBox",
              propertyType: typeof(bool),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata(defaultValue: true)
        );

        public static readonly DependencyProperty DisableGridPositioningProperty =
            DependencyProperty.Register(
              name: "DisableGridPositioning",
              propertyType: typeof(bool),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata(defaultValue: false, PropertyChangedRedrawNecessary)
        );

        public static readonly DependencyProperty ShowImageOverlayLayersProperty =
            DependencyProperty.Register(
              name: "ShowImageOverlayLayers",
              propertyType: typeof(bool),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata(defaultValue: false, PropertyChangedRedrawNecessary)
        );

        public static readonly DependencyProperty SymbolUnitWidthProperty =
            DependencyProperty.Register(
              name: "SymbolUnitWidth",
              propertyType: typeof(double),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata(defaultValue: 240d, SymbolUnitWidthPropertyChanged, OnSymbolUnitWidthCoerce)
        );

        public static readonly DependencyProperty CanvasEditModeProperty =
            DependencyProperty.Register(
              name: "CanvasEditMode",
              propertyType: typeof(CanvasEditMode),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata(
                  defaultValue: CanvasEditMode.Default,
                  flags: FrameworkPropertyMetadataOptions.None
                  )
        );

        private static void PropertyChangedRedrawNecessary(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SymbolArtRenderer renderer)
            {
                // changing if tone curve is enabled necessitates redrawing everything
                renderer.Redraw();
            }
        }

        private static void SymbolUnitWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SymbolArtRenderer renderer)
            {
                renderer.OnPropertyChanged(nameof(SymbolScaleFactor));
            }
        }

        private static object OnSymbolUnitWidthCoerce(DependencyObject d, object baseValue)
        {
            if (baseValue is double newValue)
            {
                if (newValue < AppModel.MinimumSymbolUnitWidth)
                    return AppModel.MinimumSymbolUnitWidth;
                else if (newValue > AppModel.MaximumSymbolUnitWidth)
                    return AppModel.MaximumSymbolUnitWidth;
            }
            return baseValue;
        }

        /// <summary>
        /// The radius of a layer vertex (in screen units) where a click will be considered
        /// to have hit that vertex.
        /// </summary>
        private const int LayerVertexClickRadius = 15;

        /// <summary>
        /// Set if this instance of the renderer is static and does not respond to changes in the attached symbol art.
        /// </summary>
        private readonly bool _isStatic;

        /// <summary>
        /// Disables all interaction with the control
        /// </summary>
        private bool _noInteraction;

        private Dictionary<SymbolArtItemModel, LayerModelReference> _layerDictionary
            = new();

        public SymbolArtRenderer(bool isStatic, bool noInteraction)
            : this()
        {
            _isStatic = isStatic;
            _noInteraction = noInteraction;
        }

        public SymbolArtRenderer()
        {
            InitializeComponent();

            DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(FrameworkElement))
                .AddValueChanged(this, (_, __) => OnPropertyChanged(nameof(SymbolScaleFactor)));
        }

        private ManipulationOperation operation;

        private Point rotatingOrigin;
        private double rotatingOriginAngle;

        private int draggingVertexIndex;
        private Point draggingClickOrigin;
        private Point draggingLayerOriginalPos;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Current factor between symbol art units and WPF units
        /// </summary>
        public double SymbolScaleFactor
            => ActualWidth / SymbolUnitWidth;

        public SymbolArtModel? SymbolArt
        {
            get => (SymbolArtModel?)GetValue(SymbolArtProperty);
            set => SetValue(SymbolArtProperty, value);
        }

        public bool NoInteraction
        {
            get => _noInteraction;
            set => _noInteraction = value;
        }

        public bool ShowBoundingBox
        {
            get => (bool)GetValue(ShowBoundingBoxProperty);
            set => SetValue(ShowBoundingBoxProperty, value);
        }

        public bool ShowImageOverlayLayers
        {
            get => (bool)GetValue(ShowImageOverlayLayersProperty);
            set => SetValue(ShowImageOverlayLayersProperty, value);
        }

        /// <summary>
        /// Position of the viewing area relative to the center (0,0) (symbol art units)
        /// </summary>
        public Point ViewPosition
        {
            get => (Point)GetValue(ViewPositionProperty);
            set => SetValue(ViewPositionProperty, value);
        }

        /// <summary>
        /// Represents the width of the viewport in symbol art units. 
        /// IE, decreasing the value zooms in.
        /// </summary>
        public double SymbolUnitWidth
        {
            get => (double)GetValue(SymbolUnitWidthProperty);
            set => SetValue(SymbolUnitWidthProperty, value);
        }

        public Point MouseSymbolPosition
        {
            get => (Point)GetValue(MouseSymbolPositionProperty);
            set => SetValue(MouseSymbolPositionProperty, value);
        }

        public SymbolArtItemModel? SelectedLayer
        {
            get => (SymbolArtItemModel)GetValue(SelectedLayerProperty);
            set => SetValue(SelectedLayerProperty, value);
        }

        public bool ApplyToneCurve
        {
            get => (bool)GetValue(ApplyToneCurveProperty);
            set => SetValue(ApplyToneCurveProperty, value);
        }

        public bool ShowGuides
        {
            get => (bool)GetValue(ShowGuidesProperty);
            set => SetValue(ShowGuidesProperty, value);
        }

        public bool DisableGridPositioning
        {
            get => (bool)GetValue(DisableGridPositioningProperty);
            set => SetValue(DisableGridPositioningProperty, value);
        }

        public CanvasEditMode CanvasEditMode
        {
            get => (CanvasEditMode)GetValue(CanvasEditModeProperty);
            set => SetValue(CanvasEditModeProperty, value);
        }

        /// <summary>
        /// Converts the coordinate system from relative to the view to the one used in the symbol art
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private Point CoordinatesToSymbolArt(Point target, bool includeViewOffset)
        {
            // the coordinate system for the symbol arts starts with 0,0 at the center
            // so we need to subtract half of the width/height of the view and take
            // into account the current view position
            var result = new Point(
                ((target.X - viewport3d.ActualWidth / 2 ) / SymbolScaleFactor),
                ((target.Y - viewport3d.ActualHeight / 2) / SymbolScaleFactor)
                );

            if (includeViewOffset)
            {
                result.X -= ViewPosition.X;
                result.Y -= ViewPosition.Y;
            }

            return result;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (_noInteraction)
            {
                base.OnPreviewKeyDown(e);
                return;
            }

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

                    case Key.System:
                        if (e.KeyboardDevice.IsKeyDown(Key.LeftAlt))
                        {
                            SelectedLayer.ShowBoundingVertices = true;
                            e.Handled = true;
                        }
                        else
                        {
                            SelectedLayer.ShowBoundingVertices = false;
                        }
                        break;
                }
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            if (_noInteraction)
            {
                base.OnPreviewKeyUp(e);
                return;
            }

            Cursor = null;

            if (SelectedLayer != null)
            {
                switch (e.Key)
                {
                    case Key.System:
                        SelectedLayer.ShowBoundingVertices = false;
                        break;
                }
            }

            base.OnPreviewKeyUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!_noInteraction)
            {
                // we want the zoom to increase non-linearly as otherwise it takes much longer to zoom
                // out (increase the symbol unit width) compared to zooming in (decrease the symbol unit width)
                double newWidth = SymbolUnitWidth - Math.Log(SymbolUnitWidth) * e.Delta / 100;

                SymbolUnitWidth = Math.Clamp(newWidth, AppModel.MinimumSymbolUnitWidth, AppModel.MaximumSymbolUnitWidth);
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs args)
        {
            if (_noInteraction)
            {
                base.OnPreviewMouseDown(args);
                return;
            }

            if (CanvasEditMode != CanvasEditMode.Default)
            {
                switch (CanvasEditMode)
                {
                    case CanvasEditMode.ColorPicker:
                        var color = Helpers.ScreenColorPicker.GetColorAt(PointToScreen(args.GetPosition(this)));

                        if (SelectedLayer != null)
                        {
                            SelectedLayer.Color = ApplyToneCurve ? SymbolArtColorHelper.RemoveCurve(color) : color;
                        }
                        break;
                }

                base.OnPreviewMouseDown(args);
                return;
            }

            Focus();
            Keyboard.Focus(this);

            operation = ManipulationOperation.None;

            Point ptMouse = CoordinatesToSymbolArt(args.GetPosition(viewport3d), true);

            if (args.RightButton == MouseButtonState.Pressed)
            {
                // drag viewport
                operation = ManipulationOperation.MoveView;
                draggingLayerOriginalPos = ViewPosition;
                draggingClickOrigin = args.GetPosition(viewport3d);
                Cursor = Cursors.ScrollAll;
            }
            else if (args.MiddleButton == MouseButtonState.Pressed)
            {
                TrySelectItemUnderPoint(ptMouse);
                return;
            }
            else
            {
                // if no layer is selected, there's nothing to manipulate
                if (SelectedLayer == null)
                    return;

                draggingClickOrigin = ptMouse;

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.LeftAlt))
                    {
                        var parent = SelectedLayer.Parent;

                        if (parent == null)
                            return;

                        // make a copy of the currently selected item and drag it
                        // this makes it very easy to duplicate symbols/groups
                        operation = ManipulationOperation.DragDuplicate;
                        draggingLayerOriginalPos = SelectedLayer.Position;
                        var previousLayer = SelectedLayer;
                        var duplicate = previousLayer.Duplicate(parent);
                        int currentIndex = previousLayer.IndexInParent;

                        duplicate.Undo.Do($"Duplicate {duplicate.ItemTypeName}",
                            () =>
                            {
                                parent.Children.Insert(currentIndex, duplicate);
                                SelectedLayer = duplicate;
                            },
                            () =>
                            {
                                parent.Children.Remove(duplicate);
                                SelectedLayer = previousLayer;
                            }
                        );
                    }
                    else
                    {
                        operation = ManipulationOperation.Rotate;
                        rotatingOrigin = SelectedLayer.Vertices.GetCenter();
                        rotatingOriginAngle = Math.Atan2(draggingClickOrigin.Y - rotatingOrigin.Y, draggingClickOrigin.X - rotatingOrigin.X);
                    }
                }
                else
                {
                    // get the distance from the mouse location to each vertex of the target layer
                    var distanceToVertices = SelectedLayer.Vertices
                        .Select(point => (point - draggingClickOrigin).Length)
                        .ToArray();

                    // find the closest vertex
                    draggingVertexIndex = distanceToVertices.GetMinIndexBy(x => x);

                    // check if the closest vertex is close enough that we'll consider that it was clicked
                    if (distanceToVertices[draggingVertexIndex] * SymbolScaleFactor > LayerVertexClickRadius)
                    {
                        // drag entire symbol if we're not close enough to any vertex
                        // unless alt is pressed for resize
                        if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.LeftAlt))
                        {
                            return;
                        }

                        draggingLayerOriginalPos = SelectedLayer.Position;
                        operation = ManipulationOperation.MoveItem;
                    }
                    else
                    {
                        operation = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.LeftAlt)
                            ? ManipulationOperation.Resize
                            : ManipulationOperation.DragVertex;

                        SelectedLayer.ShowBoundingVertices = operation == ManipulationOperation.Resize;
                    }
                }

                if (operation != ManipulationOperation.DragDuplicate)
                {
                    SelectedLayer.StartManipulation(operation switch
                    {
                        ManipulationOperation.DragVertex => "Manipulate",
                        ManipulationOperation.MoveItem => "Move",
                        _ => operation.ToString()
                    });
                }
            }

            CaptureMouse();
            args.Handled = true;
        }

        private bool TrySelectItemUnderPoint(Point ptMouse)
        {
            if (SymbolArt != null)
            {
                byte[] pixelValues = new byte[4];

                // select layer under cursor by traversing all layers in the symbol art
                // until we find one that is visible and where the mouse pointer is inside
                foreach (var layer in SymbolArt.GetAllLayers().Where(x => x.IsVisible))
                {
                    if (layer.IsPointInside(ptMouse))
                    {
                        var topLength = (layer.Vertex1 - layer.Vertex4).Length;
                        var leftHeight = (layer.Vertex1 - layer.Vertex2).Length;
                        var bottomLength = (layer.Vertex2 - layer.Vertex3).Length;
                        var rightHeight = (layer.Vertex4 - layer.Vertex3).Length;

                        var origin = layer.Vertices.GetCenter();

                        var angle = Math.Atan2(layer.Vertex4.Y - layer.Vertex1.Y, layer.Vertex4.X - layer.Vertex1.X);
                        var originVector = new Vector(origin.X, origin.Y);

                        //var s = Math.Sin(angle);
                        //var c = Math.Cos(angle);

                        //var translated = new Point(layer.Vertex1.X + origin.X, layer.Vertex1.Y + origin.Y);
                        //var n = new Point(translated.X * c - translated.Y * s, translated.X * s + translated.Y * c)
                        //    - originVector;

                        var newPoints = new Point[]
                        {
                            new(1, 1),
                            new(1, 11),
                            new(11, 11),
                            new(11, 1)
                        };

                        //var distances = layer.Vertices.Select(point => 1 / Math.Pow((point - ptMouse).Length, 2)).ToArray();
                        //var vectors = layer.Vertices.Select((point, index) => newPoints[index] - point).ToArray();

                        //var x12d = (layer.Vertex1 - layer.Vertex2).Length;
                        //var x13d = (layer.Vertex1 - layer.Vertex3).Length;
                        //var x14d = (layer.Vertex1 - layer.Vertex4).Length;

                        //var total = distances.Sum();

                        //distances = distances.Select(x => x / total).ToArray();

                        //var averagedVectors = vectors.Select((x, index) => x * distances[index]).ToList();
                        //var vector = averagedVectors.Aggregate((a, b) => a + b);

                        //System.Diagnostics.Debug.WriteLine($"v0d {distances[0]}, v1d {distances[1]}, v2d {distances[2]}, v3d {distances[3]}, vector {vector}");
                        //System.Diagnostics.Debug.WriteLine($"vector {vector}, pos {vector + ptMouse}");

                        //var sourceVectors = layer.Vertices.Select(v => ptMouse - v).ToArray();
                        //var targetVectors = layer.Vertices.Select((point, index) => newPoints[index] - point).ToArray();

                        //var top =    layer.Vertex4.X - layer.Vertex1.X;
                        //var bottom = layer.Vertex3.X - layer.Vertex2.X;
                        //var left =   layer.Vertex2.Y - layer.Vertex1.Y;
                        //var right =  layer.Vertex3.Y - layer.Vertex4.Y;

                        //var ptYleft = sourceVectors[0].Y / left;
                        //var ptYright = -sourceVectors[2].Y / right;
                        //var ptXtop = sourceVectors[0].X / top;
                        //var ptXbottom = sourceVectors[1].X / bottom;

                        //System.Diagnostics.Debug.WriteLine($"yleft = {ptYleft}, yright = {ptYright}, xtop = {ptXtop}, xbottom = {ptXbottom}");

                        //var point = new Point(
                        //    (10 * ptXtop + 10 * ptXbottom) / 2,
                        //    (10 * ptYleft + 10 * ptYright) / 2
                        //    );



                        //var xdist = 1 - distances[0] / (layer.Vertex1 - layer.Vertex3).Length;
                        //var ydist = 1 - distances[1] / (layer.Vertex2 - layer.Vertex4).Length;
                        //var zdist = 1 - distances[2] / (layer.Vertex1 - layer.Vertex3).Length;
                        //var wdist = 1 - distances[3] / (layer.Vertex2 - layer.Vertex4).Length;

                        //var xvec = newPoints[2] - newPoints[0];
                        //var yvec = newPoints[3] - newPoints[1];

                        //var point = xvec * xdist;
                        //var pointy = newPoints[3] - yvec * ydist;

                        //System.Diagnostics.Debug.WriteLine($"point = {point}, {pointy}, xDist = {xdist}, yDist = {ydist}, zDist = {zdist}, wDist = {wdist}");

                        //var v = layer.Vertex2 - layer.Vertex1;
                        //var u = layer.Vertex3 - layer.Vertex2;
                        //var w = new Point((v + u).X, (v + u).Y) - layer.Vertex4;

                        var p0 = layer.Vertex2;
                        var p1 = layer.Vertex3;
                        var p2 = layer.Vertex4;
                        var p3 = layer.Vertex1;

                        var normals = new Vector[]
                        {
                            new Vector(-(p3.Y - p0.Y), -(p3.X - p0.X)),
                            new Vector(-(p0.Y - p1.Y), -(p0.X - p1.X)),
                            new Vector(-(p1.Y - p2.Y), -(p1.X - p2.X)),
                            new Vector(-(p2.Y - p3.Y), -(p2.X - p3.X)),
                        };



                        var u = (ptMouse - p0) * normals[0] / ((ptMouse - p0) * normals[0] + (ptMouse - p2) * normals[2]);
                        var v = (ptMouse - p0) * normals[1] / ((ptMouse - p0) * normals[1] + (ptMouse - p3) * normals[3]);

                        System.Diagnostics.Debug.WriteLine($"u = {u}, v = {v}");

                        var point = new Point(u * 10, (1-v) * 10);

                        //MatrixEx C = MatrixEx.General2DProjection(layer.Vertices, newPoints);

                        ////var distances = layer.Vertices.Select(point => (point - ptMouse).Length).ToArray();

                        //var point = C.Transform(ptMouse);

                        //System.Diagnostics.Debug.WriteLine($"point {point}");

                        //foreach (var vertex in layer.Vertices)
                        //{
                        //    System.Diagnostics.Debug.WriteLine(C.Transform(vertex));
                        //}

                        var targetLayer = SymbolArt.GetAllLayers().FirstOrDefault(x => x.Name == "target");

                        if (targetLayer != null)
                        {
                            targetLayer.SetVertex(0, point);
                            targetLayer.SetVertex(1, point);
                            targetLayer.SetVertex(2, point);
                            targetLayer.SetVertex(3, point);
                        }


                        //var ns = SymbolManipulationHelper.Rotate(new[] { layer.Vertex1, ptMouse }, origin, -angle);
                        //var n = ns[0];

                        //var vector = ns[1] - ns[0];

                        //var percentX = Math.Abs(vector.X) / topLength;
                        //var percentY = Math.Abs(vector.Y) / leftHeight;
                        //var percentX2 = Math.Abs(vector.X) / bottomLength;
                        //var percentY2 = Math.Abs(vector.Y) / rightHeight;

                        //int targetPixelX = (int)Math.Round(layer.Symbol.Image.PixelWidth * percentX);
                        //int targetPixelY = (int)Math.Round(layer.Symbol.Image.PixelHeight * percentY);

                        ////System.Diagnostics.Debug.WriteLine($"{targetPixelX}:{targetPixelY}, {percentX:0.00}, {percentY:0.00}, {percentX2:0.00}, {percentY2:0.00}");
                        //System.Diagnostics.Debug.WriteLine($"C = {point}, v1 = {A.Transform(layer.Vertex1)}");

                        //if (targetPixelX < layer.Symbol.Image.PixelWidth - 1 && targetPixelY < layer.Symbol.Image.PixelHeight - 1)
                        //{
                        //    layer.Symbol.Image.CopyPixels(new Int32Rect(targetPixelX, targetPixelY, 1, 1), pixelValues, 4, 0);

                        //    if (pixelValues[3] > 10)
                        //    {

                        //        // just in case
                        //        SelectedLayer?.CommitManipulation();
                        //        SelectedLayer = layer;
                        //        return true;
                        //    }
                        //}
                    }
                }
            }

            return false;
        }

        private static Point UpdatePointWithVertexMoved(Point[] vertices, int vertexIndex, Point newVertexLocation, Point pointToTranslate)
        {
            var originVertex = vertices[vertexIndex];
            var oppositeVertex = vertices.GetOppositeVertex(vertexIndex);

            Vector vector = newVertexLocation - originVertex;

            if (vector.Length == 0)
            {
                return pointToTranslate;
            }

            vertices[vertexIndex] = newVertexLocation;

            var width = Math.Max(originVertex.X, oppositeVertex.X) - Math.Min(originVertex.X, oppositeVertex.X);
            var height = Math.Max(originVertex.Y, oppositeVertex.Y) - Math.Min(originVertex.Y, oppositeVertex.Y);

            // find the distance from the x and y origins of the group for the vertex
            var distanceFromOpposite = (pointToTranslate - oppositeVertex);

            // and reduce the vector to add accordingly
            var scale = Math.Abs(distanceFromOpposite.X / height) * Math.Abs(distanceFromOpposite.Y / width);

            return pointToTranslate + new Vector(vector.X * scale, vector.Y * scale);
        }

        protected override void OnMouseMove(MouseEventArgs args)
        {
            base.OnMouseMove(args);

            if (_noInteraction)
            {
                return;
            }

            Point ptMouse = CoordinatesToSymbolArt(args.GetPosition(viewport3d), true);

            MouseSymbolPosition = new Point(Math.Round(ptMouse.X), Math.Round(ptMouse.Y));

            if (operation == ManipulationOperation.None && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                TrySelectItemUnderPoint(ptMouse);
                return;
            }

            if (operation == ManipulationOperation.MoveView)
            {
                // distance traveled by mouse in screen units, not symbol art units
                var rawDiff = args.GetPosition(viewport3d) - draggingClickOrigin;

                // update view position
                ViewPosition = draggingLayerOriginalPos + new Vector(rawDiff.X / SymbolScaleFactor, rawDiff.Y / SymbolScaleFactor);
                
                args.Handled = true;
                return;
            }

            if (SelectedLayer == null)
                return;

            switch (operation)
            {
                case ManipulationOperation.Rotate:
                    var angleNow = Math.Atan2(ptMouse.Y - rotatingOrigin.Y, ptMouse.X - rotatingOrigin.X);

                    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                    {
                        SelectedLayer.TemporaryRotate(Math.Round(angleNow / (Math.PI / 4)) * (Math.PI / 4));
                    }
                    else
                    {
                        SelectedLayer.TemporaryRotate(angleNow - rotatingOriginAngle);
                    }
                    break;

                case ManipulationOperation.DragVertex:
                    SelectedLayer.SetVertex(draggingVertexIndex, ptMouse);
                    break;

                case ManipulationOperation.Resize:
                    SelectedLayer.ResizeFromVertex(draggingVertexIndex, ptMouse, Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
                    break;

                case ManipulationOperation.MoveItem:
                case ManipulationOperation.DragDuplicate:
                    var diff = ptMouse - draggingClickOrigin;

                    if (SelectedLayer.EnforceGridPositioning && !DisableGridPositioning)
                    {
                        // When moving a layer, round of the vector so that items can only be moved on the 
                        // true symbol art grid
                        SelectedLayer.Position = draggingLayerOriginalPos + new Vector(Math.Round(diff.X), Math.Round(diff.Y));
                    }
                    else
                    {
                        SelectedLayer.Position = draggingLayerOriginalPos + diff;
                    }
                    break;

                case ManipulationOperation.MoveView:
                    
                    break;

                default:
                    return;
            }

            args.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs args)
        {
            base.OnMouseUp(args);

            if (_noInteraction)
            {
                return;
            }

            if (operation != ManipulationOperation.None)
            {
                SelectedLayer?.CommitManipulation();
                operation = ManipulationOperation.None;
                ReleaseMouseCapture();
                args.Handled = true;
            }

            Cursor = null;
        }

        private void Redraw()
        {
            symbolArtContentGroup.Children.Clear();
            symbolArtContentGroup.Children.Add(new AmbientLight(Colors.White));
            _layerDictionary.Clear();

            if (SymbolArt != null)
            {
                DrawSymbolArtGroup(SymbolArt);
            }
        }

        private void DrawSymbolArtGroup(SymbolArtItemModel group)
        {
            for (int i = group.Children.Count - 1; i >= 0; i--)
            {
                if (group.Children[i] is SymbolArtLayerModel || group.Children[i] is SymbolArtImageLayerModel)
                {
                    Draw3d(group.Children[i]);
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

            if (!_isStatic)
            {
                group.Children.CollectionChanged -= Children_CollectionChanged;
                group.Children.CollectionChanged += Children_CollectionChanged;
            }
        }

        private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (SymbolArtItemModel deletedItem in e.OldItems!)
                    {
                        foreach (var item in deletedItem.GetAllItems())
                        {
                            if (_layerDictionary.TryGetValue(item, out LayerModelReference? refs))
                            {
                                symbolArtContentGroup.Children.Remove(refs.Model);
                                _layerDictionary.Remove(item);
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
                if (item is SymbolArtLayerModel || item is SymbolArtImageLayerModel)
                {
                    RefreshLayerVisibility(item);
                }
                else
                {
                    RecursiveRefreshVisibility(item);
                }
            }
        }

        private BitmapImage? GetLayerImage(SymbolArtItemModel layer)
        {
            if (layer is SymbolArtLayerModel symbolLayer)
            {
                return symbolLayer.Symbol?.Image;
            }
            else if (layer is SymbolArtImageLayerModel imageLayer)
            {
                return ShowImageOverlayLayers ? imageLayer.Image : null;
            }
            else
            {
                return null;
            }
        }

        private void Draw3d(SymbolArtItemModel layer)
        {
            // GeometryModel3D does not support data binding :(

            var brush = new ImageBrush(GetLayerImage(layer))
            {
                Opacity = layer.IsVisible ? layer.Alpha : 0
            };

            var material = new DiffuseMaterial()
            {
                Brush = brush,
                AmbientColor = ApplyToneCurve ? SymbolArtColorHelper.ApplyCurve(layer.Color) : layer.Color
            };

            var geometry = new MeshGeometry3D()
            {
                Positions = GetLayer3DPoints(layer),
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

            if (!_isStatic)
            {
                layer.PropertyChanged -= Layer_PropertyChanged;
                layer.PropertyChanged += Layer_PropertyChanged;
            }
        }

        private Point3DCollection GetLayer3DPoints(SymbolArtItemModel layer)
        {
            var vertices = layer.RawVertices;

            if (layer.EnforceGridPositioning && !DisableGridPositioning)
            {
                return new Point3DCollection
                {
                    new Point3D(Math.Round(vertices[1].X), -Math.Round(vertices[1].Y), 0),
                    new Point3D(Math.Round(vertices[0].X), -Math.Round(vertices[0].Y), 0),
                    new Point3D(Math.Round(vertices[2].X), -Math.Round(vertices[2].Y), 0),
                    new Point3D(Math.Round(vertices[3].X), -Math.Round(vertices[3].Y), 0)
                };
            }
            else
            {
                return new Point3DCollection
                {
                    new Point3D(vertices[1].X, -vertices[1].Y, 0),
                    new Point3D(vertices[0].X, -vertices[0].Y, 0),
                    new Point3D(vertices[2].X, -vertices[2].Y, 0),
                    new Point3D(vertices[3].X, -vertices[3].Y, 0)
                };
            }
        }

        private void Layer_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not SymbolArtItemModel layer)
                return;

            var refs = _layerDictionary[layer];

            switch (e.PropertyName)
            {
                case nameof(layer.Visible):
                case nameof(layer.Alpha):
                    RefreshLayerVisibility(layer);
                    break;

                case nameof(SymbolArtLayerModel.Symbol):
                    if (layer is not SymbolArtLayerModel symbolLayer)
                        return;

                    refs.Brush.ImageSource = symbolLayer.Symbol?.Image;
                    break;

                case nameof(layer.Color):
                    refs.Material.AmbientColor = ApplyToneCurve ? SymbolArtColorHelper.ApplyCurve(layer.Color) : layer.Color;
                    break;

                default:
                    if (e.PropertyName?.StartsWith("Vertex") == true || e.PropertyName == nameof(layer.RawVertices))
                    {
                        refs.Geometry.Positions = GetLayer3DPoints(layer);
                    }
                    break;
            }
        }

        private void RefreshLayerVisibility(SymbolArtItemModel layer)
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
            if (!_noInteraction)
            {
                Focus();
                Keyboard.Focus(this);
            }
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

        private enum ManipulationOperation
        {
            None,
            DragVertex,
            MoveItem,
            MoveView,
            Resize,
            Rotate,
            DragDuplicate
        }
    }
}
