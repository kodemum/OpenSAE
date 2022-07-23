using OpenSAE.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OpenSAE.Models
{
    public class SymbolArtLayerModel : SymbolArtItemModel, ISymbolArtItem
    {
        private string? _name;
        private double _alpha;
        private Color _color;
        private int _symbol;
        private bool _visible;
        private Point _vertex1;
        private Point _vertex2;
        private Point _vertex3;
        private Point _vertex4;
        private bool _showBoundingVertices;

        public SymbolArtLayerModel(SymbolArtLayer layer, SymbolArtItemModel? parent)
        {
            _name = layer.Name ?? string.Empty;
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

        public SymbolArtLayerModel(string name, SymbolArtItemModel parent)
        {
            _name = name;
            _alpha = 1;
            _color = Color.FromRgb(0xf4, 0xd7, 0x00);
            _symbol = 240;
            _visible = true;
            _vertex1 = new Point(-16, -16);
            _vertex2 = new Point(-16, 16);
            _vertex3 = new Point(16, -16);
            _vertex4 = new Point(16, 16);
            Parent = parent;
        }

        public override string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public Symbol? Symbol
        {
            get => SymbolUtil.GetById(_symbol + 1)!;
            set => SetProperty(ref _symbol, value?.Id - 1 ?? 0);
        }

        public override bool Visible
        {
            get => _visible;
            set => SetProperty(ref _visible, value);
        }

        public override bool IsVisible => Parent!.IsVisible && Visible;

        public string? SymbolPackUri => Symbol?.Uri;

        public double Alpha
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

        public Color Color
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

        /// <summary>
        /// Gets or sets the position of the entire symbol. The origin of the position
        /// is the leftmost vertex
        /// </summary>
        public Point Position
        {
            get => Vertices.GetMinBy(true);
            set
            {
                var points = Vertices;
                
                int minIndex = points.GetMinIndexBy(true);

                // find diff between previous min point and the new one
                var diff = value - points[minIndex];

                for (int i = 0; i < points.Length; i++)
                {
                    points[i] += diff;
                }

                Vertices = points;

                OnPropertyChanged(nameof(PositionX));
                OnPropertyChanged(nameof(PositionY));
            }
        }

        public short PositionX
        {
            get => (short)Math.Round(Position.X);
            set => Position = new Point(value, Position.Y);
        }

        public short PositionY
        {
            get => (short)Math.Round(Position.Y);
            set => Position = new Point(Position.X, value);
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

        public IEnumerable<Point3D> Points3D
        {
            get
            {
                yield return new Point3D(Math.Round(Vertex2.X), -Math.Round(Vertex2.Y), 0);
                yield return new Point3D(Math.Round(Vertex1.X), -Math.Round(Vertex1.Y), 0);
                yield return new Point3D(Math.Round(Vertex4.X), -Math.Round(Vertex4.Y), 0);
                yield return new Point3D(Math.Round(Vertex3.X), -Math.Round(Vertex3.Y), 0);
            }
        }

        public override SymbolArtItemModel Duplicate(SymbolArtItemModel parent)
        {
            return new SymbolArtLayerModel((SymbolArtLayer)ToSymbolArtItem(), parent);
        }

        public override SymbolArtItem ToSymbolArtItem()
        {
            return new SymbolArtLayer()
            {
                Name = _name,
                Alpha = _alpha,
                Color = _color,
                SymbolId = _symbol,
                Visible = _visible,
                Vertex1 = _vertex1,
                Vertex2 = _vertex2,
                Vertex3 = _vertex3,
                Vertex4 = _vertex4,
            };
        }

        public override void FlipX()
        {
            Vertices = SymbolManipulationHelper.FlipX(Vertices);
        }

        public override void FlipY()
        {
            Vertices = SymbolManipulationHelper.FlipY(Vertices);
        }

        public override void Rotate(double angle)
        {
            Vertices = SymbolManipulationHelper.Rotate(Vertices, angle);
        }

        public void TemporaryRotate(double angle)
        {
            TemporaryRotate(angle, Vertices.GetCenter());
        }

        public void TemporaryRotate(double angle, Point origin)
        {
            StartManipulation();

            Vertices = SymbolManipulationHelper.Rotate(_temporaryVertices!, origin, angle);
        }

        public void SetVertex(int vertexIndex, Point point)
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
                    new Point(maxX, minY),
                    new Point(maxX, maxY)
                };
            }
        }

        public bool ShowBoundingVertices
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

        public void ResizeFromVertex(int vertexIndex, Point point)
        {
            StartManipulation();

            var boundVertices = BoundingVertices;

            // find the origin and opposite vertex - this is necessary
            // in order to calculate the vector for each vertex 
            var originVertex = boundVertices[vertexIndex];
            var oppositeVertex = boundVertices[vertexIndex switch
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
