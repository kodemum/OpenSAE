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

        public SymbolArtLayerModel(IUndoModel undoModel, SymbolArtLayer layer, SymbolArtItemModel? parent)
            : base(undoModel)
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

        public SymbolArtLayerModel(IUndoModel undoModel, int index, SymbolArtItemModel parent)
            : base(undoModel)
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
                base.Name = value;
                OnPropertyChanged(nameof(DisplayName));
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

        public override string ItemTypeName => "symbol";

        public Symbol? Symbol
        {
            get => SymbolUtil.GetById(_symbol + 1)!;
            set => SetPropertyWithUndo(_symbol, value?.Id - 1 ?? 0, (x) => SetProperty(ref _symbol, x), "Change symbol");
        }

        public override bool IsVisible => Parent!.IsVisible && Visible;

        public string? SymbolPackUri => Symbol?.Uri;

        public override double Alpha
        {
            get => _alpha;
            set => SetPropertyWithUndo(_alpha, Math.Round(value * 7) / 7, x => SetProperty(ref _alpha, x), "Change symbol opacity");
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
            set => SetPropertyWithUndo(_color, value, (x) => SetProperty(ref _color, x), "Change symbol color");
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
                    OnPropertyChanged(nameof(RawVertices));
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
                    OnPropertyChanged(nameof(RawVertices));
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
                    OnPropertyChanged(nameof(RawVertices));
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
                    OnPropertyChanged(nameof(RawVertices));
                }
            }
        }

        public override Point[] RawVertices => new[]
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
                    return RawVertices;
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

        public Point[] PreManipulationVertices => _isManipulating ? _temporaryVertices : Vertices;

        public override SymbolArtItemModel Duplicate(SymbolArtItemModel parent)
        {
            var duplicate = (SymbolArtLayer)ToSymbolArtItem();
            duplicate.Index = GetMaxLayerIndex() + 1;

            return new SymbolArtLayerModel(_undoModel, duplicate, parent);
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

        public bool IsPointInsideAndNotTransparent(Point ptMouse)
        {
            var p0 = Vertex2;
            var p1 = Vertex3;
            var p2 = Vertex4;
            var p3 = Vertex1;

            var n0 = new Vector(-(p3.Y - p0.Y), (p3.X - p0.X));
            var n1 = new Vector((p0.Y - p1.Y), -(p0.X - p1.X));
            var n2 = new Vector(-(p1.Y - p2.Y), (p1.X - p2.X));
            var n3 = new Vector((p2.Y - p3.Y), -(p2.X - p3.X));

            n0.Normalize();
            n1.Normalize();
            n2.Normalize();
            n3.Normalize();

            var u = (ptMouse - p0) * n0 / ((ptMouse - p0) * n0 + (ptMouse - p2) * n2);
            var v = (ptMouse - p0) * n1 / ((ptMouse - p0) * n1 + (ptMouse - p3) * n3);

            if (Symbol == null || double.IsNaN(u) || double.IsNaN(v) || u < 0 || u > 1 || v < 0 || v > 1)
                return false;

            int targetPixelX = (int)Math.Round((Symbol.Image.PixelWidth - 1) * u);
            int targetPixelY = (int)Math.Round((Symbol.Image.PixelHeight - 1) * (1 - v));

            byte[] pixelValues = new byte[4];

            Symbol.Image.CopyPixels(new Int32Rect(targetPixelX, targetPixelY, 1, 1), pixelValues, 4, 0);

            return pixelValues[3] > 50;
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
    }
}
