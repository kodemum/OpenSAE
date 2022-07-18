using OpenSAE.Core;
using System;
using System.Linq;
using System.Windows;

namespace OpenSAE.Models
{
    public class SymbolArtGroupModel : SymbolArtItemModel, ISymbolArtItem
    {
        private readonly ISymbolArtGroup _group;

        public SymbolArtGroupModel(ISymbolArtGroup group, SymbolArtItemModel parent)
        {
            _group = group;
            Parent = parent;

            foreach (var item in _group.Children)
            {
                if (item is ISymbolArtGroup subGroup)
                {
                    Children.Add(new SymbolArtGroupModel(subGroup, this));
                }
                else if (item is SymbolArtLayer layer)
                {
                    Children.Add(new SymbolArtLayerModel(layer, this));
                }
                else
                {
                    throw new Exception($"Item of unknown type {item.GetType().Name} found in symbol art group");
                }
            }
        }

        public override string? Name
        {
            get => _group.Name;
            set
            {
                _group.Name = value;
                OnPropertyChanged();
            }
        }

        public override bool Visible
        {
            get => _group.Visible;
            set
            {
                _group.Visible = value;
                OnPropertyChanged();
            }
        }

        public override bool IsVisible => Parent!.IsVisible && Visible;

        public SymbolArtPoint Vertex1 => Vertices[0];
        public SymbolArtPoint Vertex2 => Vertices[1];
        public SymbolArtPoint Vertex3 => Vertices[2];
        public SymbolArtPoint Vertex4 => Vertices[3];

        public SymbolArtPoint[] Vertices
        {
            get
            {
                var layers = GetAllLayers().ToArray();

                // we'll just assume the 4 points are the 4 extreme points of any in the group/subgroups
                var allPoints = layers.SelectMany(x => x.Vertices).ToArray();

                double minX = allPoints.MinBy(x => x.X).X, maxX = allPoints.MaxBy(x => x.X).X;
                double minY = allPoints.MinBy(x => x.Y).Y, maxY = allPoints.MaxBy(x => x.Y).Y;

                return new[]
                {
                    new SymbolArtPoint(minX, minY),
                    new SymbolArtPoint(minX, maxY),
                    new SymbolArtPoint(maxX, minY),
                    new SymbolArtPoint(maxX, maxY)
                };
            }
        }

        /// <summary>
        /// Gets or sets the position of the entire symbol. The origin of the position
        /// is the leftmost vertex
        /// </summary>
        public SymbolArtPoint Position
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
                OnPropertyChanged(nameof(Vertex1));
                OnPropertyChanged(nameof(Vertex2));
                OnPropertyChanged(nameof(Vertex3));
                OnPropertyChanged(nameof(Vertex4));
            }
        }

        /// <summary>
        /// Sets the specified vertex to a new position, shifting all vertices in all layers in the group
        /// accordingly, effectively resizing the group as a whole.
        /// </summary>
        /// <param name="vertexIndex">Index of vertex to change position for. (0-3)</param>
        /// <param name="point">New location for the vertex</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetVertex(int vertexIndex, SymbolArtPoint point)
        {
            // find the origin and opposite vertex - this is necessary
            // in order to calculate the vector for each vertex of each layer
            // in this group
            var originVertex = (Point)Vertices[vertexIndex];
            var oppositeVertex = (Point)Vertices[vertexIndex switch
            {
                0 => 3,
                1 => 2,
                2 => 1,
                3 => 0,
                _ => throw new ArgumentException("Vertex must be in the range 0-3")
            }];

            Vector vector = point - originVertex;

            if (vector.Length == 0)
            {
                return;
            }

            var layers = GetAllLayers().ToArray();

            // get the bounds of the group
            var width = Math.Max(originVertex.X, oppositeVertex.X) - Math.Min(originVertex.X, oppositeVertex.X);
            var height = Math.Max(originVertex.Y, oppositeVertex.Y) - Math.Min(originVertex.Y, oppositeVertex.Y);

            foreach (var layer in layers)
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
            OnPropertyChanged(nameof(Vertex1));
            OnPropertyChanged(nameof(Vertex2));
            OnPropertyChanged(nameof(Vertex3));
            OnPropertyChanged(nameof(Vertex4));
        }
    }
}
