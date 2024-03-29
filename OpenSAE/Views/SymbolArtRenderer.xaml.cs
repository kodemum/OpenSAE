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

        public static readonly DependencyProperty SelectedLayersProperty =
            DependencyProperty.Register(
              name: "SelectedLayers",
              propertyType: typeof(IReadOnlyList<SymbolArtItemModel>),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata(
                  defaultValue: null,
                  flags: FrameworkPropertyMetadataOptions.AffectsRender,
                  SelectedLayersPropertyChanged
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

        public static readonly DependencyProperty DisplaySettingFlagsProperty =
            DependencyProperty.Register(
              name: "DisplaySettingFlags",
              propertyType: typeof(DisplaySettingFlags),
              ownerType: typeof(SymbolArtRenderer),
              typeMetadata: new FrameworkPropertyMetadata(defaultValue: DisplaySettingFlags.NaturalSymbolSelection, PropertyChangedRedrawNecessary)
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

        private static void SelectedLayersPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SymbolArtRenderer renderer)
            {
                renderer.UpdateSelectedLayer();
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

        private SymbolArtItemModel? _selectedLayer;

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
            get => _selectedLayer;
            set
            {
                var list = new List<SymbolArtItemModel>();

                if (value != null)
                {
                    list.Add(value);
                }

                SelectedLayers = list;
            }
        }

        public IReadOnlyList<SymbolArtItemModel>? SelectedLayers
        {
            get => (IReadOnlyList<SymbolArtItemModel>?)GetValue(SelectedLayersProperty);
            set
            {
                SetValue(SelectedLayersProperty, value);
                OnPropertyChanged(nameof(SelectedLayer));
            }
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

        public DisplaySettingFlags DisplaySettingFlags
        {
            get => (DisplaySettingFlags)GetValue(DisplaySettingFlagsProperty);
            set => SetValue(DisplaySettingFlagsProperty, value);
        }

        private void UpdateSelectedLayer()
        {
            if (SelectedLayers == null || SelectedLayers.Count == 0)
            {
                _selectedLayer = null;
            }
            else if (SelectedLayers?.Count == 1)
            {
                _selectedLayer = SelectedLayers[0];
            }
            else
            {
                _selectedLayer = SymbolArtItemCollection.Create(SelectedLayers); ;
            }

            OnPropertyChanged(nameof(SelectedLayer));
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

            if (CanvasEditMode != CanvasEditMode.Default && args.LeftButton == MouseButtonState.Pressed)
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
                var naturalSelect = DisplaySettingFlags.HasFlag(DisplaySettingFlags.NaturalSymbolSelection);

                // select layer under cursor by traversing all layers in the symbol art
                // until we find one that is visible and where the mouse pointer is inside
                foreach (var layer in SymbolArt.GetAllLayers().Where(x => x.IsVisible))
                {
                    if (naturalSelect ? layer.IsPointInsideAndNotTransparent(ptMouse) : layer.IsPointInside(ptMouse))
                    {
                        // just in case
                        SelectedLayer?.CommitManipulation();

                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            // add layer to selection
                            var layers = new List<SymbolArtItemModel>(SelectedLayers);
                            layers.Add(layer);
                            SelectedLayers = layers;
                        }
                        else
                        {
                            // replace selection with layer
                            SelectedLayers = new List<SymbolArtItemModel>() { layer };
                        }
                        return true;
                    }
                }
            }

            return false;
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
                        SelectedLayer.TemporaryRotate(Math.Round(angleNow));
                    }
                    else
                    {
                        SelectedLayer.TemporaryRotate(angleNow - rotatingOriginAngle);
                    }
                    break;

                case ManipulationOperation.DragVertex:
                    var orgVertex = SelectedLayer.Vertices[draggingVertexIndex];
                    int companionIndex = draggingVertexIndex == 3 ? 0 : draggingVertexIndex + 1;

                    SelectedLayer.SetVertex(draggingVertexIndex, ptMouse);
                    if (DisplaySettingFlags.HasFlag(DisplaySettingFlags.RestrictToAffineManipulation))
                    {
                        SelectedLayer.SetVertex(companionIndex, SelectedLayer.Vertices[companionIndex] + (ptMouse - orgVertex));
                    }
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

        /// <summary>
        /// Clears and redraws all symbols
        /// </summary>
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

        /// <summary>
        /// Refreshes the order of all symbols, but does not recreate their objects
        /// </summary>
        private void RefreshOrder()
        {
            var list = new Model3DCollection
            {
                new AmbientLight(Colors.White)
            };

            if (SymbolArt != null)
            {
                foreach (var layer in SymbolArt.GetAllItems().Reverse())
                {
                    if (layer is SymbolArtLayerModel || layer is SymbolArtImageLayerModel)
                    {
                        _layerDictionary[layer].AddTo(list);
                    }
                }
            }

            symbolArtContentGroup.Children = list;
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

            if (!_isStatic)
            {
                group.Children.CollectionChanged -= Children_CollectionChanged;
                group.Children.CollectionChanged += Children_CollectionChanged;
                group.PropertyChanged -= GroupPropertyChanged;
                group.PropertyChanged += GroupPropertyChanged;
            }
        }

        private void GroupPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(SymbolArtItemModel.Visible) && sender is SymbolArtItemModel group)
            {
                RecursiveRefreshVisibility(group);
            }
        }

        private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            bool reorderNeeded = false;

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (SymbolArtItemModel deletedItem in e.OldItems!)
                    {
                        foreach (var item in deletedItem.GetAllItems())
                        {
                            if (_layerDictionary.TryGetValue(item, out LayerModelReference? refs))
                            {
                                refs.RemoveFrom(symbolArtContentGroup.Children);
                                _layerDictionary.Remove(item);
                            }
                        }
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (SymbolArtItemModel addedItem in e.NewItems!)
                    {
                        if (addedItem is SymbolArtGroupModel group)
                        {
                            DrawSymbolArtGroup(group);
                        }
                        else
                        {
                            Draw3d(addedItem);
                        }
                    }
                    reorderNeeded = true;
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    reorderNeeded = true;
                    break;

                default:
                    // for any other operation lets redraw everything
                    Redraw();
                    break;
            }

            if (reorderNeeded)
            {
                RefreshOrder();
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

        private BitmapSource? GetLayerImage(SymbolArtItemModel layer)
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
                Opacity = layer.IsVisible ? layer.Alpha : 0,
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

            LayerModelReference reference;

            if (DisplaySettingFlags.HasFlag(DisplaySettingFlags.IngameRenderMode))
            {
                var positions1 = GetLayer3DPoints(layer);
                var coordinates1 = new PointCollection(geometry.TextureCoordinates);
                positions1.RemoveAt(1);
                coordinates1.RemoveAt(1);

                var model1 = new GeometryModel3D()
                {
                    Geometry = new MeshGeometry3D()
                    {
                        Positions = positions1,
                        TextureCoordinates = coordinates1,
                        TriangleIndices = geometry.TriangleIndices
                    },
                    Material = material,
                    BackMaterial = material,
                };

                var positions2 = GetLayer3DPoints(layer);
                var coordinates2 = new PointCollection(geometry.TextureCoordinates);
                positions2.RemoveAt(2);
                coordinates2.RemoveAt(2);

                var model2 = new GeometryModel3D()
                {
                    Geometry = new MeshGeometry3D()
                    {
                        Positions = positions2,
                        TextureCoordinates = coordinates2,
                        TriangleIndices = geometry.TriangleIndices
                    },
                    Material = material,
                    BackMaterial = material,
                };

                reference = new LayerModelReference(model1, model2, material, brush);
            }
            else
            {
                var model = new GeometryModel3D()
                {
                    Geometry = geometry,
                    Material = material,
                    BackMaterial = material,
                };

                reference = new LayerModelReference(model, material, brush);
            }

            _layerDictionary.Add(layer, reference);

            reference.AddTo(symbolArtContentGroup.Children);

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
                        refs.UpdateGeometry(GetLayer3DPoints(layer));
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
            public LayerModelReference(GeometryModel3D model, DiffuseMaterial material, ImageBrush brush)
            {
                Model = model;
                Material = material;
                Brush = brush;
                Geometry = model.Geometry as MeshGeometry3D ?? throw new InvalidOperationException("Only MeshGeometry3D supported");
            }

            public LayerModelReference(GeometryModel3D model1, GeometryModel3D model2, DiffuseMaterial material, ImageBrush brush)
            {
                Model = model1;
                Model2 = model2;
                Material = material;
                Brush = brush;
                Geometry = model1.Geometry as MeshGeometry3D ?? throw new InvalidOperationException("Only MeshGeometry3D supported");
                Geometry3D2 = model2.Geometry as MeshGeometry3D ?? throw new InvalidOperationException("Only MeshGeometry3D supported");
            }

            private GeometryModel3D Model { get; }

            private GeometryModel3D? Model2 { get; }

            public DiffuseMaterial Material { get; }

            public ImageBrush Brush { get; }

            private MeshGeometry3D Geometry { get; }

            private MeshGeometry3D? Geometry3D2 { get; }

            public void UpdateGeometry(Point3DCollection point3DCollection)
            {
                if (Geometry3D2 == null)
                {
                    Geometry.Positions = point3DCollection;
                }
                else
                {
                    var positions1 = new Point3DCollection(point3DCollection);
                    var positions2 = new Point3DCollection(point3DCollection);
                    positions1.RemoveAt(1);
                    positions2.RemoveAt(2);

                    Geometry.Positions = positions1;
                    Geometry3D2.Positions = positions2;
                }
            }

            public void AddTo(Model3DCollection collection)
            {
                collection.Add(Model);

                if (Model2 != null)
                {
                    collection.Add(Model2);
                }
            }

            public void RemoveFrom(Model3DCollection collection)
            {
                collection.Remove(Model);

                if (Model2 != null)
                {
                    collection.Remove(Model2);
                }
            }
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
