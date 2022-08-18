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
            get
            {
                if (Symbol?.Vectors != null)
                {
                    return _vertex1 - Symbol.Vectors[0];
                }
                else
                {
                    return _vertex1;
                }
            }
            set
            {
                if (Symbol?.Vectors != null)
                {
                    value += Symbol.Vectors[0];
                }

                if (SetProperty(ref _vertex1, value))
                {
                    OnPropertyChanged(nameof(Vertices));
                    OnPropertyChanged(nameof(RawVertices));
                }
            }
        }

        protected override Vector[]? Vectors => Symbol?.Vectors;

        /// <summary>
        /// Represents the left bottom vertex of the symbol
        /// </summary>
        public Point Vertex2
        {
            get
            {
                if (Symbol?.Vectors != null)
                {
                    return _vertex2 - Symbol.Vectors[1];
                }
                else
                {
                    return _vertex2;
                }
            }
            set
            {
                if (Symbol?.Vectors != null)
                {
                    value += Symbol.Vectors[1];
                }

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
            get
            {
                if (Symbol?.Vectors != null)
                {
                    return _vertex3 - Symbol.Vectors[2];
                }
                else
                {
                    return _vertex3;
                }
            }
            set
            {
                if (Symbol?.Vectors != null)
                {
                    value += Symbol.Vectors[2];
                }


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
            get
            {
                if (Symbol?.Vectors != null)
                {
                    return _vertex4 - Symbol.Vectors[3];
                }
                else
                {
                    return _vertex4;
                }
            }
            set
            {
                if (Symbol?.Vectors != null)
                {
                    value += Symbol.Vectors[3];
                }

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

        public override Point[] RenderVertices => new[]
        {
            _vertex1,
            _vertex2,
            _vertex3,
            _vertex4
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

        public PointCollection PointCollection => new(Vertices);

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
