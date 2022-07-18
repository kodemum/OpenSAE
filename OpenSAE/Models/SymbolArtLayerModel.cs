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
        private readonly SymbolArtLayer _layer;

        public SymbolArtLayerModel(SymbolArtLayer layer, SymbolArtItemModel parent)
        {
            _layer = layer;
            Parent = parent;
        }

        public override string? Name
        {
            get => _layer.Name;
            set
            {
                _layer.Name = value;
                OnPropertyChanged();
            }
        }

        public int Symbol
        {
            get => _layer.Type;
            set
            {
                _layer.Type = value;
                OnPropertyChanged();
            }
        }

        public override bool Visible
        {
            get => _layer.Visible;
            set
            {
                _layer.Visible = value;
                OnPropertyChanged();
            }
        }

        public override bool IsVisible => Parent!.IsVisible && Visible;

        public string? SymbolPackUri => SymbolUtil.GetSymbolPackUri(_layer.Type);

        public double Alpha
        {
            get => _layer.Alpha;
            set
            {
                _layer.Alpha = value;
                OnPropertyChanged();
            }
        }

        public Color ColorWithAlpha
        {
            get
            {
                var color = Color;

                color.A = (byte)Math.Round(_layer.Alpha * 255);

                return color;
            }
            set
            {
                Color = value;
                Alpha = Math.Round((double)value.A / 255 * 7) / 7;
                OnPropertyChanged();
            }
        }

        public Color Color
        {
            get => (Color)ColorConverter.ConvertFromString(_layer.Color ?? "#ffffff");
            set
            {
                _layer.Color = string.Format("#{0:x2}{1:x2}{2:x2}", value.R, value.G, value.B);
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColorWithAlpha));
            }
        }

        /// <summary>
        /// Represents the left top vertex of the symbol
        /// </summary>
        public SymbolArtPoint Vertex1
        {
            get => _layer.Vertex1;
            set
            {
                _layer.Vertex1 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Vertex1X));
                OnPropertyChanged(nameof(Vertex1Y));
                OnPropertyChanged(nameof(Vertices));
            }
        }

        /// <summary>
        /// Represents the left bottom vertex of the symbol
        /// </summary>
        public SymbolArtPoint Vertex2
        {
            get => _layer.Vertex2;
            set
            {
                _layer.Vertex2 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Vertex2X));
                OnPropertyChanged(nameof(Vertex2Y));
                OnPropertyChanged(nameof(Vertices));
            }
        }

        /// <summary>
        /// Represents the right top vertex of the symbol
        /// </summary>
        public SymbolArtPoint Vertex3
        {
            get => _layer.Vertex3;
            set
            {
                _layer.Vertex3 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Vertex3X));
                OnPropertyChanged(nameof(Vertex3Y));
                OnPropertyChanged(nameof(Vertices));
            }
        }

        /// <summary>
        /// Represents the right bottom vertex of the symbol
        /// </summary>
        public SymbolArtPoint Vertex4
        {
            get => _layer.Vertex4;
            set
            {
                _layer.Vertex4 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Vertex4X));
                OnPropertyChanged(nameof(Vertex4Y));
                OnPropertyChanged(nameof(Vertices));
            }
        }

        public short Vertex1X
        {
            get => Vertex1.RoundedX;
            set => Vertex1 = new SymbolArtPoint(value, Vertex1.RoundedY);
        }

        public short Vertex1Y
        {
            get => Vertex1.RoundedY;
            set => Vertex1 = new SymbolArtPoint(Vertex1.RoundedX, value);
        }

        public short Vertex2X
        {
            get => Vertex2.RoundedX;
            set => Vertex2 = new SymbolArtPoint(value, Vertex2.RoundedY);
        }

        public short Vertex2Y
        {
            get => Vertex2.RoundedY;
            set => Vertex2 = new SymbolArtPoint(Vertex2.RoundedX, value);
        }

        public short Vertex3X
        {
            get => Vertex3.RoundedX;
            set => Vertex3 = new SymbolArtPoint(value, Vertex3.RoundedY);
        }

        public short Vertex3Y
        {
            get => Vertex3.RoundedY;
            set => Vertex3 = new SymbolArtPoint(Vertex3.RoundedX, value);
        }

        public short Vertex4X
        {
            get => Vertex4.RoundedX;
            set => Vertex4 = new SymbolArtPoint(value, Vertex4.RoundedY);
        }

        public short Vertex4Y
        {
            get => Vertex4.RoundedY;
            set => Vertex4 = new SymbolArtPoint(Vertex4.RoundedX, value);
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
            get => Position.RoundedX;
            set => Position = new SymbolArtPoint(value, Position.RoundedY);
        }

        public short PositionY
        {
            get => Position.RoundedY;
            set => Position = new SymbolArtPoint(Position.RoundedX, value);
        }

        public SymbolArtPoint[] Vertices
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
                yield return new Point3D(Vertex2.RoundedX, -Vertex2.RoundedY, 0);
                yield return new Point3D(Vertex1.RoundedX, -Vertex1.RoundedY, 0);
                yield return new Point3D(Vertex4.RoundedX, -Vertex4.RoundedY, 0);
                yield return new Point3D(Vertex3.RoundedX, -Vertex3.RoundedY, 0);
            }
        }

        public void SetVertex(int vertexIndex, SymbolArtPoint point)
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
