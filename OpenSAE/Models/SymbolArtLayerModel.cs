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
        private readonly SymbolArtLayer _layer;
        private readonly SymbolArtItemModel _parent;

        public SymbolArtLayerModel(SymbolArtLayer layer, SymbolArtItemModel parent)
        {
            _layer = layer;
            _parent = parent;
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

        public override bool IsVisible => _parent.IsVisible && Visible;

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
            get => new(_layer.Ltx, _layer.Lty);
            set
            {
                _layer.Ltx = value.X;
                _layer.Lty = value.Y;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Vertex1X));
                OnPropertyChanged(nameof(Vertex1Y));
            }
        }

        /// <summary>
        /// Represents the left bottom vertex of the symbol
        /// </summary>
        public SymbolArtPoint Vertex2
        {
            get => new(_layer.Lbx, _layer.Lby);
            set
            {
                _layer.Lbx = value.X;
                _layer.Lby = value.Y;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Vertex2X));
                OnPropertyChanged(nameof(Vertex2Y));
            }
        }

        /// <summary>
        /// Represents the right top vertex of the symbol
        /// </summary>
        public SymbolArtPoint Vertex3
        {
            get => new(_layer.Rtx, _layer.Rty);
            set
            {
                _layer.Rtx = value.X;
                _layer.Rty = value.Y;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Vertex3X));
                OnPropertyChanged(nameof(Vertex3Y));
            }
        }

        /// <summary>
        /// Represents the right bottom vertex of the symbol
        /// </summary>
        public SymbolArtPoint Vertex4
        {
            get => new(_layer.Rbx, _layer.Rby);
            set
            {
                _layer.Rbx = value.X;
                _layer.Rby = value.Y;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Vertex4X));
                OnPropertyChanged(nameof(Vertex4Y));
            }
        }

        public short Vertex1X
        {
            get => Vertex1.X;
            set => Vertex1 = new SymbolArtPoint(value, Vertex1.Y);
        }

        public short Vertex1Y
        {
            get => Vertex1.Y;
            set => Vertex1 = new SymbolArtPoint(Vertex1.X, value);
        }

        public short Vertex2X
        {
            get => Vertex2.X;
            set => Vertex2 = new SymbolArtPoint(value, Vertex2.Y);
        }

        public short Vertex2Y
        {
            get => Vertex2.Y;
            set => Vertex2 = new SymbolArtPoint(Vertex2.X, value);
        }

        public short Vertex3X
        {
            get => Vertex3.X;
            set => Vertex3 = new SymbolArtPoint(value, Vertex3.Y);
        }

        public short Vertex3Y
        {
            get => Vertex3.Y;
            set => Vertex3 = new SymbolArtPoint(Vertex3.X, value);
        }

        public short Vertex4X
        {
            get => Vertex4.X;
            set => Vertex4 = new SymbolArtPoint(value, Vertex4.Y);
        }

        public short Vertex4Y
        {
            get => Vertex4.Y;
            set => Vertex4 = new SymbolArtPoint(Vertex4.X, value);
        }

        /// <summary>
        /// Gets or sets the position of the entire symbol. The origin of the position
        /// is the leftmost vertex
        /// </summary>
        public SymbolArtPoint Position
        {
            get => Points.GetMinBy(true);
            set
            {
                var points = Points;
                
                int minIndex = points.GetMinIndexBy(true);

                // find diff between previous min point and the new one
                var diff = value - points[minIndex];

                for (int i = 0; i < points.Length; i++)
                {
                    points[i] += diff;
                }

                Points = points;
            }
        }

        public short PositionX
        {
            get => Position.X;
            set => Position = new SymbolArtPoint(value, Position.Y);
        }

        public short PositionY
        {
            get => Position.Y;
            set => Position = new SymbolArtPoint(Position.X, value);
        }

        public SymbolArtPoint[] Points
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

        public PointCollection PointCollection => new(Points.Select(x => new Point(x.X, x.Y)));

        public IEnumerable<Point3D> Points3D
        {
            get
            {
                yield return new Point3D(_layer.Lbx, -_layer.Lby, 0);
                yield return new Point3D(_layer.Ltx, -_layer.Lty, 0);
                yield return new Point3D(_layer.Rbx, -_layer.Rby, 0);
                yield return new Point3D(_layer.Rtx, -_layer.Rty, 0);
            }
        }
    }
}
