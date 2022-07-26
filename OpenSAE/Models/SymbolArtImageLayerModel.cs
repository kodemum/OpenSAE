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

        public SymbolArtImageLayerModel(string name, byte[] imageData, SymbolArtItemModel parent)
        {
            _name = name;
            ImageData = imageData ?? throw new ArgumentNullException(nameof(imageData));
            _isVisible = true;
            _alpha = 0.8;

            // fit the image in the bounds of the symbol art and center it
            Vertices = SymbolManipulationHelper.CenterArbitrarySizeInArea(192, 96, Image!.Width, Image!.Height);
            Parent = parent;
        }

        public SymbolArtImageLayerModel(SymbolArtBitmapImageLayer layer, SymbolArtItemModel parent)
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
            set => SetProperty(ref _alpha, value);
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
            throw new NotImplementedException();
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

        public override void SetVertex(int vertexIndex, Point point)
        {
            StartManipulation();

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
