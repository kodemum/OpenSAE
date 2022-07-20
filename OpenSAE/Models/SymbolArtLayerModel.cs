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
        private string _color;
        private int _symbol;
        private bool _visible;
        private Point _vertex1;
        private Point _vertex2;
        private Point _vertex3;
        private Point _vertex4;
        private Point[]? _temporaryVertices;
        private bool _isRotating;

        public SymbolArtLayerModel(SymbolArtLayer layer, SymbolArtItemModel? parent)
        {
            _name = layer.Name ?? string.Empty;
            _alpha = layer.Alpha;
            _color = layer.Color ?? "#ffffff";
            _symbol = layer.Type;
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
            _color = "#f4d700";
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
            get => (Color)ColorConverter.ConvertFromString(_color);
            set
            {
                if (SetProperty(ref _color, string.Format("#{0:x2}{1:x2}{2:x2}", value.R, value.G, value.B)))
                {
                    OnPropertyChanged(nameof(ColorWithAlpha));
                }
            }
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
                    OnPropertyChanged(nameof(Vertex1X));
                    OnPropertyChanged(nameof(Vertex1Y));
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
                    OnPropertyChanged(nameof(Vertex2X));
                    OnPropertyChanged(nameof(Vertex2Y));
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
                    OnPropertyChanged(nameof(Vertex3X));
                    OnPropertyChanged(nameof(Vertex3Y));
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
                    OnPropertyChanged(nameof(Vertex4X));
                    OnPropertyChanged(nameof(Vertex4Y));
                    OnPropertyChanged(nameof(Vertices));
                }
            }
        }

        public short Vertex1X
        {
            get => (short)Math.Round(Vertex1.X);
            set => Vertex1 = new Point(value, Vertex1.Y);
        }

        public short Vertex1Y
        {
            get => (short)Math.Round(Vertex1.Y);
            set => Vertex1 = new Point(Vertex1.X, value);
        }

        public short Vertex2X
        {
            get => (short)Math.Round(Vertex2.X);
            set => Vertex2 = new Point(value, Vertex2.Y);
        }

        public short Vertex2Y
        {
            get => (short)Math.Round(Vertex2.Y);
            set => Vertex2 = new Point(Vertex2.X, value);
        }

        public short Vertex3X
        {
            get => (short)Math.Round(Vertex3.X);
            set => Vertex3 = new Point(value, Vertex3.Y);
        }

        public short Vertex3Y
        {
            get => (short)Math.Round(Vertex3.Y);
            set => Vertex3 = new Point(Vertex3.X, value);
        }

        public short Vertex4X
        {
            get => (short)Math.Round(Vertex4.X);
            set => Vertex4 = new Point(value, Vertex4.Y);
        }

        public short Vertex4Y
        {
            get => (short)Math.Round(Vertex4.Y);
            set => Vertex4 = new Point(Vertex4.X, value);
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

        public Point[] Vertices
        {
            get
            {
                return new[]
                {
                    Vertex1,
                    Vertex2,
                    Vertex3,
                    Vertex4,
                };
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

        public PointCollection PointCollection => new(Vertices.Select(x => (Point)x));

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
                Type = _symbol,
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
            if (!_isRotating)
            {
                _temporaryVertices = Vertices;
                _isRotating = true;
            }

            Vertices = SymbolManipulationHelper.Rotate(_temporaryVertices!, origin, angle);
        }

        public void CommitRotate()
        {
            if (_isRotating)
            {
                _isRotating = false;
                _temporaryVertices = null;
            }
        }

        public void SetVertex(int vertexIndex, Point point)
        {
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
    }
}
