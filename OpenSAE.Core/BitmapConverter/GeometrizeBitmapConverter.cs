using geometrize;
using geometrize.bitmap;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenSAE.Core.BitmapConverter
{
    public sealed class GeometrizeBitmapConverter : IDisposable
    {
        private readonly Image<Rgba32> _originalImage;
        private readonly double _scale;
        private readonly double _xOffset;
        private readonly double _yOffset;
        private readonly BitmapToSymbolArtConverterOptions _options;

        public GeometrizeBitmapConverter(string filename, BitmapToSymbolArtConverterOptions options)
        {
            Filename = filename;
            _options = options;

            _originalImage = Image.Load<Rgba32>(filename);

            if (options.RespectEdges)
            {
                _originalImage.Mutate(x => x
                    .Resize(new ResizeOptions()
                    {
                        Size = new Size(options.ResizeImageHeight * 2, options.ResizeImageHeight),
                        Sampler = KnownResamplers.Lanczos8,
                        Mode = ResizeMode.Pad,
                        PadColor = Color.Transparent,
                    })
                    .BackgroundColor(Color.White)
                );

                _scale = 192d / _originalImage.Width;
                _xOffset = 96;
                _yOffset = 48;
            }
            else
            {
                _originalImage.Mutate(x => x
                    .Resize(new ResizeOptions()
                    {
                        Size = new Size(options.ResizeImageHeight, options.ResizeImageHeight),
                        Sampler = KnownResamplers.Lanczos8,
                        Mode = ResizeMode.Max
                    })
                    .BackgroundColor(Color.White)
                    );

                if ((double)_originalImage.Width / _originalImage.Height > 2)
                {
                    _scale = 192d / _originalImage.Width;
                    _xOffset = 96;
                    _yOffset = _originalImage.Height * _scale / 2;
                }
                else
                {
                    _scale = 96d / _originalImage.Height;
                    _xOffset = _originalImage.Width * _scale / 2;
                    _yOffset = 48;
                }
            }
        }

        public object Filename { get; }

        public void Convert(Action<SymbolArtLayer> layerDone, CancellationToken token, IProgress<double>? progress = null)
        {
            if (_options.ShapeTypes.Length == 0)
                return;

            byte[] imageData = new byte[_originalImage.Width * _originalImage.Height * 4];

            _originalImage.CopyPixelDataTo(imageData);

            Bitmap bitmap = Bitmap.createFromBytes(_originalImage.Width, _originalImage.Height, new haxe.io.Bytes(imageData.Length, imageData));

            var commonColor = FindMostCommonColor();

            Model geometrize = new(bitmap, GeometrizeUtil.ColorToInt(commonColor));

            for (int count = 0; count < _options.MaxSymbolCount; )
            {
                token.ThrowIfCancellationRequested();

                if (_options.IncludeBackground && count == 0)
                {
                    // background is always 100% opaque
                    layerDone.Invoke(ToLayer(
                        new GeometrizeShape(ShapeType.Rectangle, commonColor, 0, new double[] { 0, 0, _originalImage.Width, _originalImage.Height }),
                        1));

                    count++;
                }
                else
                {
                    var items = geometrize.step(
                        new HaxeArray<int>(_options.ShapeTypes.Select(x => (int)x).ToArray()),
                        (int)Math.Round(_options.SymbolOpacity * 255),
                        _options.ShapesPerStep,
                        _options.MutationsPerStep);

                    for (int i = 0; i < items.length; i++)
                    {
                        layerDone.Invoke(ToLayer(GeometrizeUtil.ConvertShape(items[i]), _options.SymbolOpacity));

                        count++;
                    }
                }

                progress?.Report((double)count / _options.MaxSymbolCount * 100);
            }
        }

        private SymbolArtLayer ToLayer(GeometrizeShape shape, double opacity)
        {
            System.Windows.Point[] vertices;
            int symbolId;

            (symbolId, vertices) = shape.Type switch
            {
                ShapeType.Circle => ConvertCircle(shape),
                ShapeType.Rectangle or ShapeType.Rotated_Rectangle => ConvertRectangle(shape),
                ShapeType.Ellipse or ShapeType.Rotated_Ellipse => ConvertEllipse(shape),
                _ => throw new NotImplementedException($"Shape {shape.Type} is not supported."),
            };

            return new SymbolArtLayer()
            {
                Alpha = opacity,
                Color = SymbolArtColorHelper.RemoveCurve(shape.Color),
                Visible = true,
                SymbolId = symbolId,
                Vertex1 = vertices[0],
                Vertex2 = vertices[1],
                Vertex3 = vertices[2],
                Vertex4 = vertices[3],
            };
        }

        private (int SymbolId, System.Windows.Point[] Vertices) ConvertCircle(GeometrizeShape shape)
        {
            if (shape.Points.Length != 3)
                throw new InvalidOperationException("Circles must have three points");

            double x = shape.Points[0], y = shape.Points[1], rx = shape.Points[2];

            x *= _scale;
            y *= _scale;
            rx *= _scale;

            x += rx * 0.05;
            y += rx * 0.05;
            rx *= 1.1;

            x -= _xOffset;
            y -= _yOffset;

            return (240, new System.Windows.Point[]
            {
                new System.Windows.Point(x - rx, y - rx),
                new System.Windows.Point(x - rx, y + rx),
                new System.Windows.Point(x + rx, y + rx),
                new System.Windows.Point(x + rx, y - rx)
            });
        }

        private (int SymbolId, System.Windows.Point[] Vertices) ConvertEllipse(GeometrizeShape shape)
        {
            if (shape.Type == ShapeType.Ellipse && shape.Points.Length != 4)
                throw new InvalidOperationException("Ellipses must have FOUR POINTS");
            else if (shape.Type == ShapeType.Rotated_Ellipse && shape.Points.Length != 5)
                throw new InvalidOperationException("Rotated ellipses must have five points");

            double centerX = shape.Points[0], centerY = shape.Points[1], radiusX = shape.Points[2], radiusY = shape.Points[3];

            centerX *= _scale;
            centerY *= _scale;
            radiusX *= _scale * 1.05;
            radiusY *= _scale * 1.05;

            centerX -= _xOffset;
            centerY -= _yOffset;

            var vertices = new System.Windows.Point[]
            {
                new System.Windows.Point(centerX - radiusX, centerY - radiusY),
                new System.Windows.Point(centerX - radiusX, centerY + radiusY),
                new System.Windows.Point(centerX + radiusX, centerY + radiusY),
                new System.Windows.Point(centerX + radiusX, centerY - radiusY)
            };

            if (shape.Type == ShapeType.Rotated_Ellipse)
            {
                vertices = SymbolManipulationHelper.Rotate(vertices, shape.Points[4] / 180 * Math.PI);
            }

            return (240, vertices);
        }

        private (int SymbolId, System.Windows.Point[] Vertices) ConvertRectangle(GeometrizeShape shape)
        {
            if (shape.Type == ShapeType.Rectangle && shape.Points.Length != 4)
                throw new InvalidOperationException("Rectangles must have FOUR POINTS");
            else if (shape.Type == ShapeType.Rotated_Rectangle && shape.Points.Length != 5)
                throw new InvalidOperationException("Rotated rectangles must have five points");

            double x1 = shape.Points[0], y1 = shape.Points[1], x2 = shape.Points[2], y2 = shape.Points[3];

            x1 *= _scale;
            y1 *= _scale;
            x2 *= _scale;
            y2 *= _scale;

            var width = x2 - x1;
            var height = y2 - y1;

            x1 -= width * 0.34 / 2;
            y1 -= height * 0.34 / 2;

            x2 += width * 0.34 / 2;
            y2 += height * 0.34 / 2;

            x1 -= _xOffset;
            x2 -= _xOffset;
            y1 -= _yOffset;
            y2 -= _yOffset;

            var vertices = new System.Windows.Point[]
            {
                new System.Windows.Point(x1, y1),
                new System.Windows.Point(x1, y2),
                new System.Windows.Point(x2, y2),
                new System.Windows.Point(x2, y1)
            };

            if (shape.Type == ShapeType.Rotated_Rectangle)
            {
                vertices = SymbolManipulationHelper.Rotate(vertices, shape.Points[4] / 180 * Math.PI);
            }

            return (242, vertices);
        }

        public void Dispose()
        {
            _originalImage.Dispose();
        }

        private Rgba32 FindMostCommonColor()
        {
            Dictionary<Rgba32, int> colors = new();

            _originalImage.ProcessPixelRows(accessor =>
            {
                for (int rowi = 0; rowi < accessor.Height; rowi++)
                {
                    var row = accessor.GetRowSpan(rowi);

                    for (int coli = 0; coli < row.Length; coli++)
                    {
                        var color = row[coli];

                        // only consider mostly-opaque pixels
                        if (color.A > 128)
                        {
                            if (colors.TryGetValue(color, out int count))
                            {
                                colors[color] = count + 1;
                            }
                            else
                            {
                                colors[color] = 1;
                            }
                        }
                    }
                }
            });

            return colors.OrderByDescending(x => x.Value).First().Key;
        }
    }
}
