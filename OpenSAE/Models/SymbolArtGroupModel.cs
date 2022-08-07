using OpenSAE.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace OpenSAE.Models
{
    public class SymbolArtGroupModel : SymbolArtItemModel
    {
        private string _pendingSymbolText = string.Empty;

        public SymbolArtGroupModel(IUndoModel undoModel, ISymbolArtGroup group, SymbolArtItemModel? parent)
            : this(undoModel)
        {
            Parent = parent;
            _name = group.Name;
            _visible = group.Visible;

            AddChildren(group.Children);
        }

        public SymbolArtGroupModel(IUndoModel undoModel, string name, bool visible, SymbolArtItemModel parent)
            : this(undoModel)
        {
            _name = name;
            _visible = visible;
            Parent = parent;
        }

        protected SymbolArtGroupModel(IUndoModel undoModel)
            : base(undoModel)
        {
        }

        /// <summary>
        /// Only exists to prevent databinding error - is always null
        /// </summary>
        public Symbol? Symbol
        {
            get => null;
            set { }
        }

        public string PendingSymbolText
        {
            get => _pendingSymbolText;
            set
            {
                AddTextAsSymbols(_pendingSymbolText, value);
                SetProperty(ref _pendingSymbolText, value);
            }
        }

        public override string ItemTypeName => "group";

        public override bool IsVisible => Parent!.IsVisible && Visible;

        public override Point[] Vertices
        {
            get
            {
                var layers = GetAllLayers().ToArray();

                // we'll just assume the 4 points are the 4 extreme points of any in the group/subgroups
                var allPoints = layers.SelectMany(x => x.Vertices).ToArray();

                if (allPoints.Length == 0)
                {
                    var none = new Point(0, 0);
                    
                    return new Point[]
                    {
                        none, none, none, none
                    };
                }

                double minX = allPoints.MinBy(x => x.X).X, maxX = allPoints.MaxBy(x => x.X).X;
                double minY = allPoints.MinBy(x => x.Y).Y, maxY = allPoints.MaxBy(x => x.Y).Y;

                return new[]
                {
                    new Point(minX, minY),
                    new Point(minX, maxY),
                    new Point(maxX, maxY),
                    new Point(maxX, minY)
                };
            }
            set
            {
            }
        }

        public override Point[] RawVertices => Vertices;

        public Point[] AbsoluteVertices
        {
            get
            {
                var layers = GetAllLayers().ToArray();

                // we'll just assume the 4 points are the 4 extreme points of any in the group/subgroups
                var allPoints = layers.SelectMany(x => x.Vertices).ToArray();

                if (allPoints.Length == 0)
                {
                    var none = new Point(0, 0);

                    return new Point[]
                    {
                        none, none, none, none
                    };
                }

                return new[]
                {
                    allPoints.OrderBy(x => x.X).First(),
                    allPoints.OrderByDescending(x => x.Y).First(),
                    allPoints.OrderByDescending(x => x.X).First(),
                    allPoints.OrderBy(x => x.Y).First(),
                };
            }
        }

        /// <summary>
        /// Gets or sets the position of the entire symbol. The origin of the position
        /// is the leftmost vertex
        /// </summary>
        public override Point Position
        {
            get => Vertices.GetMinBy(true);
            set
            {
                var points = Vertices;

                int minIndex = points.GetMinIndexBy(true);

                // find diff between previous min point and the new one
                var diff = value - points[minIndex];

                var layers = GetAllLayers().ToArray();

                // update all points for all layers beneath this group
                foreach (var layer in layers)
                {
                    layer.Position += diff;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(Vertices));
                OnPropertyChanged(nameof(RawVertices));
            }
        }

        public override double Alpha
        {
            get
            {
                return GetAllLayers().FirstOrDefault()?.Alpha ?? 1;
            }
            set
            {
                if (value != Alpha)
                {
                    _undoModel.BeginAggregate("Change group opacity", this, nameof(Alpha));

                    foreach (var layer in GetAllLayers())
                    {
                        layer.Alpha = value;
                    }

                    _undoModel.EndAggregate();

                    OnPropertyChanged();
                }
            }
        }

        public override Color Color
        {
            get => GetAllLayers().FirstOrDefault()?.Color ?? new Color();
            set
            {
                if (value != Color)
                {
                    _undoModel.BeginAggregate($"Change group color", this, nameof(Color));

                    foreach (var layer in GetAllLayers())
                    {
                        layer.Color = value;
                    }

                    _undoModel.EndAggregate();

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Groups always show bounding vertices
        /// </summary>
        public override bool ShowBoundingVertices
        {
            get => true;
            set
            {
            }
        }

        protected void AddChildren(IEnumerable<SymbolArtItem> items)
        {
            foreach (var item in items)
            {
                if (item is ISymbolArtGroup subGroup)
                {
                    Children.Add(new SymbolArtGroupModel(_undoModel, subGroup, this));
                }
                else if (item is SymbolArtLayer layer)
                {
                    Children.Add(new SymbolArtLayerModel(_undoModel, layer, this));
                }
                else if (item is SymbolArtBitmapImageLayer imageLayer)
                {
                    Children.Add(new SymbolArtImageLayerModel(_undoModel, imageLayer, this));
                }
                else
                {
                    throw new Exception($"Item of unknown type {item.GetType().Name} found in symbol art group");
                }
            }
        }

        public override SymbolArtItemModel Duplicate(SymbolArtItemModel parent)
        {
            var duplicateGroup = new SymbolArtGroupModel(_undoModel, Name ?? string.Empty, Visible, parent);

            // this will be recursive since child groups will also be duplicated
            foreach (var child in Children)
            {
                duplicateGroup.Children.Add(child.Duplicate(duplicateGroup));
            }

            return duplicateGroup;
        }

        public override SymbolArtItem ToSymbolArtItem()
        {
            return new SymbolArtGroup()
            {
                Name = _name,
                Visible = _visible,
                Children = Children.Select(x => x.ToSymbolArtItem()).ToList()
            };
        }

        public override void FlipX()
        {
            // find center origin
            var originX = Vertices.GetCenterX();

            foreach (var layer in GetAllLayers())
            {
                layer.Vertices = SymbolManipulationHelper.FlipX(layer.Vertices, originX);
            }
         }

        public override void FlipY()
        {
            // find center origin
            var originY = Vertices.GetCenterY();

            foreach (var layer in GetAllLayers())
            {
                layer.Vertices = SymbolManipulationHelper.FlipY(layer.Vertices, originY);
            }
        }

        public override void Rotate(double angle)
        {
            // find center origin
            var origin = Vertices.GetCenter();

            foreach (var layer in GetAllLayers())
            {
                layer.Vertices = SymbolManipulationHelper.Rotate(layer.Vertices, origin, angle);
            }

            OnPropertyChanged(nameof(Vertices));
            OnPropertyChanged(nameof(RawVertices));
            OnPropertyChanged(nameof(Position));
        }

        public override void StartManipulation(string? operationName = null)
        {
            if (!_isManipulating)
            {
                _undoModel.BeginAggregate($"{operationName ?? "Manipulate"} group", null, null, OnVerticesChanged, OnVerticesChanged);

                foreach (var layer in GetAllLayers())
                {
                    layer.StartManipulation();
                }
            }

            base.StartManipulation();
        }

        public override void TemporaryRotate(double angle)
        {
            var origin = _temporaryVertices.GetCenter();

            foreach (var layer in GetAllLayers())
            {
                layer.TemporaryRotate(angle, origin);
            }

            OnPropertyChanged(nameof(Vertices));
            OnPropertyChanged(nameof(RawVertices));
            OnPropertyChanged(nameof(Position));
        }

        public override void CommitManipulation()
        {
            if (_isManipulating)
            {
                foreach (var layer in GetAllLayers())
                {
                    layer.CommitManipulation();
                }

                base.CommitManipulation();

                _undoModel.EndAggregate();
            }
        }

        private void OnVerticesChanged()
        {
            OnPropertyChanged(nameof(Vertices));
            OnPropertyChanged(nameof(RawVertices));
            OnPropertyChanged(nameof(Position));
        }

        /// <summary>
        /// Sets the specified vertex to a new position, shifting all vertices in all layers in the group
        /// accordingly, effectively resizing the group as a whole.
        /// </summary>
        /// <param name="vertexIndex">Index of vertex to change position for. (0-3)</param>
        /// <param name="point">New location for the vertex</param>
        /// <exception cref="ArgumentException"></exception>
        public override void ResizeFromVertex(int vertexIndex, Point point)
        {
            // find the origin and opposite vertex - this is necessary
            // in order to calculate the vector for each vertex of each layer
            // in this group
            var originVertex = Vertices[vertexIndex];
            var oppositeVertex = Vertices.GetOppositeVertex(vertexIndex);

            Vector vector = point - originVertex;

            // get the bounds of the group
            var width = Math.Max(originVertex.X, oppositeVertex.X) - Math.Min(originVertex.X, oppositeVertex.X);
            var height = Math.Max(originVertex.Y, oppositeVertex.Y) - Math.Min(originVertex.Y, oppositeVertex.Y);

            // ensure it is not possible to resize the group below 1x1 by clearing the 
            // x and or y axis of the vector according to the direction being resized
            var absVectorX = vertexIndex == 0 || vertexIndex == 1 ? vector.X : -vector.X;
            var absVectorY = vertexIndex == 0 || vertexIndex == 2 ? vector.Y : -vector.Y;

            if (width - absVectorX < 1)
                vector.X = 0;

            if (height - absVectorY < 1)
                vector.Y = 0;

            if (vector.Length == 0)
                return;

            foreach (var layer in GetAllLayers())
            {
                for (int i = 0; i < 4; i++)
                {
                    // for each vertex for the layer, calculate
                    var targetVertex = layer.Vertices[i];

                    // find the distance from the x and y origins of the group for the vertex
                    var distanceFromOriginX = Math.Max(originVertex.X, targetVertex.X) - Math.Min(originVertex.X, targetVertex.X);
                    var distanceFromOriginY = Math.Max(originVertex.Y, targetVertex.Y) - Math.Min(originVertex.Y, targetVertex.Y);

                    // and reduce the vector to add accordingly
                    var xScale = 1 - distanceFromOriginX / width;
                    var yScale = 1 - distanceFromOriginY / height;

                    // Explanation:
                    // Imagine that this vertex is at the absolute corner of the group (the coordinates are identical)
                    // This means that it should be moved exactly to the point specified as an argument to this function,
                    // thus the xScale and yScale are both 1.
                    //
                    // For another vertex in the group at _any other location_, it will need to be moved less in order to
                    // properly resize the group. Unless the distance to this vertex from the origin vertex is identical
                    // for X and Y, the scale factor will be different for X and Y.
                    //
                    // Imagine a vertex right in the center of the group. This vertex will have both an X and Y scale
                    // of 0.5. So it will move half as much as our imagined vertex at the corner as described above.

                    layer.SetVertex(i, targetVertex + new Vector(vector.X * xScale, vector.Y * yScale));
                }
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(Position));
            OnPropertyChanged(nameof(Vertices));
            OnPropertyChanged(nameof(RawVertices));
        }

        /// <summary>
        /// Moves the specified (virtual) vertex to the specified point, shifting all points in the group
        /// and transforming the shape of the group.
        /// </summary>
        /// <param name="vertexIndex">Index of vertex to change position for. (0-3)</param>
        /// <param name="point">New location for the vertex</param>
        /// <exception cref="ArgumentException"></exception>
        public override void SetVertex(int vertexIndex, Point point)
        {
            // find the origin and opposite vertex - this is necessary
            // in order to calculate the vector for each vertex of each layer
            // in this group
            var originVertex = _temporaryVertices[vertexIndex];
            var oppositeVertex = _temporaryVertices.GetOppositeVertex(vertexIndex);

            Vector vector = point - originVertex;

            if (vector.Length == 0)
            {
                return;
            }

            var diff = (originVertex - oppositeVertex).Length;

            foreach (var layer in GetAllLayers())
            {
                for (int i = 0; i < 4; i++)
                {
                    // for each vertex for the layer, calculate
                    var targetVertex = layer.OriginalVertices[i];

                    // find the distance from the x and y origins of the group for the vertex
                    var distanceFromOrigin = (originVertex - targetVertex).Length;

                    // and reduce the vector to add accordingly
                    var scale = 1 - distanceFromOrigin / diff * 1.2;

                    // Explanation:
                    // Imagine that this vertex is at the absolute corner of the group (the coordinates are identical)
                    // This means that it should be moved exactly to the point specified as an argument to this function,
                    // thus the xScale and yScale are both 1.
                    //
                    // For another vertex in the group at _any other location_, it will need to be moved less in order to
                    // properly resize the group. Unless the distance to this vertex from the origin vertex is identical
                    // for X and Y, the scale factor will be different for X and Y.
                    //
                    // Imagine a vertex right in the center of the group. This vertex will have both an X and Y scale
                    // of 0.5. So it will move half as much as our imagined vertex at the corner as described above.
                    layer.SetVertex(i, targetVertex + new Vector(vector.X * scale, vector.Y * scale));
                }
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(Position));
            OnPropertyChanged(nameof(Vertices));
            OnPropertyChanged(nameof(RawVertices));
        }

        public void AddTextAsSymbols(string previousText, string newText)
        {
            _undoModel.BeginAggregate("Add text", this, nameof(PendingSymbolText), OnVerticesChanged, OnVerticesChanged);

            double x = -96, y = -48;
            double sizeX = 13, sizeY = 14;

            int nextLayerIndex = GetMaxLayerIndex() + 1;

            if (previousText.Length > newText.Length)
            {
                var removed = previousText.Substring(newText.Length);

                foreach (char c in removed.Reverse())
                {
                    var characterLayer = Children.OfType<SymbolArtLayerModel>().Where(x => x.Symbol?.Name == c.ToString()).LastOrDefault();

                    if (characterLayer != null)
                        characterLayer.Delete();
                }
            }
            else
            {
                var addedText = newText.Substring(previousText.Length);

                if (addedText.Trim().Length == 0)
                {
                    return;
                }

                if (previousText.Length > 0)
                {
                    string? lastChar = previousText.Where(x => x != ' ').LastOrDefault().ToString();

                    var characterLayer = Children.OfType<SymbolArtLayerModel>().Where(x => x.Symbol?.Name == lastChar).LastOrDefault();

                    if (characterLayer != null)
                    {
                        sizeX = characterLayer.Vertex3.X - characterLayer.Vertex1.X;
                        sizeY = characterLayer.Vertex3.Y - characterLayer.Vertex1.Y;

                        x = characterLayer.Vertex3.X - (characterLayer.Symbol!.KerningRight * sizeX);
                        y = characterLayer.Vertex1.Y;
                    }
                }

                if (previousText.EndsWith(" "))
                {
                    x += sizeX * 0.7;
                }

                foreach (string c in addedText.Split())
                {
                    if (c == " ")
                    {
                        x += sizeX * 0.7;
                        continue;
                    }

                    // find symbol for character
                    var symbol = SymbolUtil.List.FirstOrDefault(x => x.Name == c);

                    if (symbol != null)
                    {
                        x -= symbol.KerningLeft * sizeX;

                        var layer = new SymbolArtLayerModel(_undoModel, nextLayerIndex++, this)
                        {
                            Symbol = symbol,
                            Color = Color,
                            Vertex1 = new Point(x, y),
                            Vertex2 = new Point(x, y + sizeY),
                            Vertex3 = new Point(x + sizeX, y + sizeY),
                            Vertex4 = new Point(x + sizeX, y)
                        };

                        Children.Add(layer);

                        x += sizeX + -(layer.Symbol.KerningRight * sizeX);
                    }
                }
            }

            _undoModel.EndAggregate();
        }
    }
}
