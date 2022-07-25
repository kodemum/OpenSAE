using OpenSAE.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OpenSAE.Models
{
    public class SymbolArtLayerModel : SymbolArtItemModel
    {
        /// <summary>
        /// Name used by the old japanese symbol art editor for new layers 'new layer'
        /// </summary>
        private const string LegacyEditorNewLayerName = "新規レイヤー";

        private double _alpha;
        private Color _color;
        private int _symbol;
        private Point _vertex1;
        private Point _vertex2;
        private Point _vertex3;
        private Point _vertex4;
        private bool _showBoundingVertices;
        private int _index;

        public SymbolArtLayerModel(SymbolArtLayer layer, SymbolArtItemModel? parent)
        {
            _index = layer.Index;
            _name = layer.Name;
            _alpha = layer.Alpha;
            _color = layer.Color;
            _symbol = layer.SymbolId;
            _visible = layer.Visible;
            _vertex1 = layer.Vertex1;
            _vertex2 = layer.Vertex2;
            _vertex3 = layer.Vertex3;
            _vertex4 = layer.Vertex4;
            Parent = parent;
        }

        public SymbolArtLayerModel(int index, SymbolArtItemModel parent)
        {
            _index = index;
            _alpha = 1;
            _color = Color.FromRgb(0xf4, 0xd7, 0x00);
            _symbol = 240;
            _visible = true;
            _vertex1 = new Point(-16, -16);
            _vertex2 = new Point(-16, 16);
            _vertex3 = new Point(16, 16);
            _vertex4 = new Point(16, -16);
            Parent = parent;
        }

        public override string? Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public override string DisplayName
        {
            get
            {
                // consider name empty if it is the standard name for new layers in the old SA editor
                if (string.IsNullOrEmpty(_name) || _name == LegacyEditorNewLayerName)
                {
                    // generate a suitable name
                    return $"[{_index}]";
                }
                else
                {
                    return _name;
                }
            }
        }

        public Symbol? Symbol
        {
            get => SymbolUtil.GetById(_symbol + 1)!;
            set
            {
                if (SetProperty(ref _symbol, value?.Id - 1 ?? 0))
                {
                    if (string.IsNullOrEmpty(_name))
                    {
                        OnPropertyChanged(nameof(Name));
                    }
                };
            }
        }

        public override bool IsVisible => Parent!.IsVisible && Visible;

        public string? SymbolPackUri => Symbol?.Uri;

        public override double Alpha
        {
            get => _alpha;
            set => SetProperty(ref _alpha, Math.Round(value * 7) / 7);
        }

        public Color ColorWithAlpha
        {
            get
            {
                var color = Color;

                color.A = (byte)Math.Round(_alpha * 255);

                return color;
            }
            set
            {
                Color = value;
                Alpha = value.A / 255;
                OnPropertyChanged();
            }
        }

        public override Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        /// <summary>
        /// Represents the left top vertex of the symbol
        /// </summary>
        public Point Vertex1
        {
            get => _vertex1;
            set
            {
                if (SetProperty(ref _vertex1, value))
                {
                    OnPropertyChanged(nameof(Vertices));
                }
            }
        }

        /// <summary>
        /// Represents the left bottom vertex of the symbol
        /// </summary>
        public Point Vertex2
        {
            get => _vertex2;
            set
            {
                if (SetProperty(ref _vertex2, value))
                {
                    OnPropertyChanged(nameof(Vertices));
                }
            }
        }

        /// <summary>
        /// Represents the right top vertex of the symbol
        /// </summary>
        public Point Vertex3
        {
            get => _vertex3;
            set
            {
                if (SetProperty(ref _vertex3, value))
                {
                    OnPropertyChanged(nameof(Vertices));
                }
            }
        }

        /// <summary>
        /// Represents the right bottom vertex of the symbol
        /// </summary>
        public Point Vertex4
        {
            get => _vertex4;
            set
            {
                if (SetProperty(ref _vertex4, value))
                {
                    OnPropertyChanged(nameof(Vertices));
                }
            }
        }

        public Point[] RawVertices => new[]
            {
                Vertex1,
                Vertex2,
                Vertex3,
                Vertex4,
            };

        public override Point[] Vertices
        {
            get
            {
                if (ShowBoundingVertices)
                {
                    return BoundingVertices;
                }
                else
                {
                    return new[]
                    {
                        Vertex1,
                        Vertex2,
                        Vertex3,
                        Vertex4,
                    };
                }
            }
            set
            {
                if (value.Length != 4)
                    throw new ArgumentException("4 points must be supplied");

                Vertex1 = value[0];
                Vertex2 = value[1];
                Vertex3 = value[2];
                Vertex4 = value[3];
            }
        }

        public PointCollection PointCollection => new(Vertices);

        public override SymbolArtItemModel Duplicate(SymbolArtItemModel parent)
        {
            var duplicate = (SymbolArtLayer)ToSymbolArtItem();
            duplicate.Index = GetMaxLayerIndex() + 1;

            return new SymbolArtLayerModel(duplicate, parent);
        }

        public override SymbolArtItem ToSymbolArtItem()
        {
            return new SymbolArtLayer()
            {
                Name = _name,
                Alpha = _alpha,
                Index = _index,
                Color = _color,
                SymbolId = _symbol,
                Visible = _visible,
                Vertex1 = _vertex1,
                Vertex2 = _vertex2,
                Vertex3 = _vertex3,
                Vertex4 = _vertex4,
            };
        }

        public override void SetVertex(int vertexIndex, Point point)
        {
            StartManipulation();

            switch (vertexIndex)
            {
                case 0:
                    Vertex1 = point;
                    break;

                case 1:
                    Vertex2 = point;
                    break;

                case 2:
                    Vertex3 = point;
                    break;

                case 3:
                    Vertex4 = point;
                    break;

                default:
                    throw new ArgumentException("Vertex index must be 0-3", nameof(point));
            }
        }

        public Point[] BoundingVertices
        {
            get
            {
                var vertices = RawVertices;

                double minX = vertices.MinBy(x => x.X).X, maxX = vertices.MaxBy(x => x.X).X;
                double minY = vertices.MinBy(x => x.Y).Y, maxY = vertices.MaxBy(x => x.Y).Y;

                return new[]
                {
                    new Point(minX, minY),
                    new Point(minX, maxY),
                    new Point(maxX, maxY),
                    new Point(maxX, minY)
                };
            }
        }

        public override bool ShowBoundingVertices
        {
            get => _showBoundingVertices;
            set
            {
                if (SetProperty(ref _showBoundingVertices, value))
                {
                    OnPropertyChanged(nameof(Vertices));
                }
            }
        }

        public override void ResizeFromVertex(int vertexIndex, Point point)
        {
            StartManipulation();

            var boundVertices = BoundingVertices;

            // find the origin and opposite vertex - this is necessary
            // in order to calculate the vector for each vertex 
            var originVertex = boundVertices[vertexIndex];
            var oppositeVertex = boundVertices.GetOppositeVertex(vertexIndex);

            Vector vector = point - originVertex;

            if (vector.Length == 0)
            {
                return;
            }

            // get the bounds
            var width = Math.Max(originVertex.X, oppositeVertex.X) - Math.Min(originVertex.X, oppositeVertex.X);
            var height = Math.Max(originVertex.Y, oppositeVertex.Y) - Math.Min(originVertex.Y, oppositeVertex.Y);

            for (int i = 0; i < 4; i++)
            {
                // for each vertex for the layer, calculate
                var targetVertex = RawVertices[i];

                // find the distance from the x and y origins of the group for the vertex
                var distanceFromOriginX = Math.Max(originVertex.X, targetVertex.X) - Math.Min(originVertex.X, targetVertex.X);
                var distanceFromOriginY = Math.Max(originVertex.Y, targetVertex.Y) - Math.Min(originVertex.Y, targetVertex.Y);

                // and reduce the vector to add accordingly
                var xScale = 1 - distanceFromOriginX / width;
                var yScale = 1 - distanceFromOriginY / height;

                SetVertex(i, targetVertex + new Vector(vector.X * xScale, vector.Y * yScale));
            }
        }
    }
}
