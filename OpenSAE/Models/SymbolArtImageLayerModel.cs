using OpenSAE.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenSAE.Models
{
    internal class SymbolArtImageLayerModel : SymbolArtItemModel
    {
        private bool _isVisible;
        private bool _showBoundingVertices;
        private double _alpha;
        private byte[]? _imageData;
        private BitmapImage? _image;
        private Point _vertex1;
        private Point _vertex2;
        private Point _vertex3;
        private Point _vertex4;

        public SymbolArtImageLayerModel(IUndoModel undoModel, string name, byte[] imageData, SymbolArtItemModel parent)
            : base(undoModel)
        {
            _name = name;
            ImageData = imageData ?? throw new ArgumentNullException(nameof(imageData));
            _isVisible = true;
            _alpha = 0.8;

            // fit the image in the bounds of the symbol art and center it
            Vertices = SymbolManipulationHelper.CenterArbitrarySizeInArea(192, 96, Image!.Width, Image!.Height);
            Parent = parent;
        }

        public SymbolArtImageLayerModel(IUndoModel undoModel, SymbolArtBitmapImageLayer layer, SymbolArtItemModel parent)
            : base(undoModel)
        {
            _name = layer.Name;
            ImageData = layer.ImageData;
            _isVisible = layer.Visible;
            _alpha = layer.Alpha;
            Parent = parent;
            Vertices = new[]
            {
                layer.Vertex1,
                layer.Vertex2,
                layer.Vertex3,
                layer.Vertex4
            };
        }

        public BitmapImage? Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        /// <summary>
        /// Only exists to prevent databinding error - is always null
        /// </summary>
        public Symbol? Symbol
        {
            get => null;
            set { }
        }

        public override bool Visible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public override bool IsVisible => Visible;

        public override string ItemTypeName => "overlay image";

        public override bool EnforceGridPositioning => false;

        public byte[]? ImageData
        {
            get => _imageData;
            set
            {
                if (SetProperty(ref _imageData, value))
                {
                    if (value != null)
                    {
                        var ms = new MemoryStream(value);

                        BitmapImage image = new();

                        image.BeginInit();
                        image.StreamSource = ms;
                        image.EndInit();

                        Image = image;
                    }
                    else
                    {
                        Image = null;
                    }
                }
            }
        }

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

                _vertex1 = value[0];
                _vertex2 = value[1];
                _vertex3 = value[2];
                _vertex4 = value[3];

                OnPropertyChanged();
                OnPropertyChanged(nameof(RawVertices));
            }
        }

        public override Point[] RawVertices => new[]
            {
                _vertex1,
                _vertex2,
                _vertex3,
                _vertex4,
            };

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

        public override double Alpha
        {
            get => _alpha;
            set => SetPropertyWithUndo(_alpha, value, (x) => SetProperty(ref _alpha, x), "Change image layer opacity");
        }

        public override Color Color
        {
            get => Colors.White;
            set
            {
            }
        }

        public override SymbolArtItemModel Duplicate(SymbolArtItemModel parent)
        {
            var duplicate = (SymbolArtBitmapImageLayer)ToSymbolArtItem();

            return new SymbolArtImageLayerModel(_undoModel, duplicate, parent);
        }

        public override void SetVertex(int vertexIndex, Point point)
        {
            switch (vertexIndex)
            {
                case 0:
                    _vertex1 = point;
                    break;

                case 1:
                    _vertex2 = point;
                    break;

                case 2:
                    _vertex3 = point;
                    break;

                case 3:
                    _vertex4 = point;
                    break;

                default:
                    throw new ArgumentException("Vertex index must be 0-3", nameof(point));
            }

            OnPropertyChanged(nameof(Vertices));
            OnPropertyChanged(nameof(RawVertices));
        }

        public override SymbolArtItem ToSymbolArtItem()
        {
            return new SymbolArtBitmapImageLayer()
            {
                ImageData = _imageData,
                Alpha = _alpha,
                Name = _name,
                Visible = Visible,
                Vertex1 = _vertex1,
                Vertex2 = _vertex2,
                Vertex3 = _vertex3,
                Vertex4 = _vertex4
            };
        }
    }
}
