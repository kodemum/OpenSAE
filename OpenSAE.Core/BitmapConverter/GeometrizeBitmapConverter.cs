using Geometrize;
using Geometrize.Shape;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OpenSAE.Core.BitmapConverter
{
    public sealed class GeometrizeBitmapConverter : IDisposable
    {
        private readonly Image<Rgba32> _originalImage;
        private readonly double _scale;
        private readonly double _xOffset;
        private readonly double _yOffset;
        private readonly BitmapToSymbolArtConverterOptions _options;
        private readonly System.Windows.Media.Color _backgroundColor;

        private static byte[,] ExtractScanLines(Symbol symbol)
        {
            byte[] rawPixelData = new byte[symbol.Image.PixelWidth * symbol.Image.PixelHeight * 4];

            symbol.Image.CopyPixels(rawPixelData, symbol.Image.PixelWidth * 4, 0);

            byte[,] results = new byte[symbol.Image.PixelWidth, symbol.Image.PixelHeight];

            for (int y = 0; y < symbol.Image.PixelHeight; y++)
            {
                for (int x = 0; x < symbol.Image.PixelWidth; x++)
                {
                    int pos = (y * symbol.Image.PixelWidth + x) * 4;

                    results[y, x] = rawPixelData[pos + 3];
                }
            }

            return results;
        }

        public GeometrizeBitmapConverter(string filename, BitmapToSymbolArtConverterOptions options)
        {
            Filename = filename;
            _options = options;

            _originalImage = Image.Load<Rgba32>(filename);

            var targetSize = options.ResizeImageHeight;
            _backgroundColor = _options.IncludeBackground ? _options.BackgroundColor : Colors.White;

            var backgroundColor = SixLabors.ImageSharp.Color.FromRgb(_backgroundColor.R, _backgroundColor.G, _backgroundColor.B);

            if (options.RespectEdges)
            {
                _originalImage.Mutate(x => x
                    .Resize(new ResizeOptions()
                    {
                        Size = new Size(targetSize * 2, targetSize),
                        Sampler = KnownResamplers.Lanczos8,
                        Mode = ResizeMode.Pad,
                        PadColor = backgroundColor,
                    })
                    .BackgroundColor(backgroundColor)
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
                        Size = new Size(targetSize, targetSize),
                        Sampler = KnownResamplers.Lanczos8,
                        Mode = ResizeMode.Max
                    })
                    .BackgroundColor(backgroundColor)
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

        public void Convert(Action<SymbolArtLayer> layerDone, CancellationToken token, IProgress<GeometrizeProgress>? progress = null, Action<Image>? inProgressImageCallback = null)
        {
            if (_options.ShapeTypes.Length == 0)
                return;

            Bitmap bitmap = Bitmap.CreateFromImage(_originalImage);

            var commonColor = _options.IncludeBackground ? _options.BackgroundColor : Colors.White;
            var symbolOptions = new SymbolShapeOptions()
            {
                ShapeTypes = _options.ShapeTypes.Select(x => (int)x).ToArray(),
                SymbolDefinitions = _options.ShapeSymbolsToUse.Select(x => new SymbolShapeDefinition()
                {
                    SymbolId = x.Id,
                    SymbolScanlines = ExtractScanLines(x),
                    HorizontallySymmetric = x.Flags.HasFlag(SymbolFlag.HorizontallySymmetric) || x.Flags.HasFlag(SymbolFlag.Symmetric),
                    VerticallySymmetric = x.Flags.HasFlag(SymbolFlag.VerticallySymmetric) || x.Flags.HasFlag(SymbolFlag.Symmetric),
                }).ToList()
            };

            if ((_options.ShapeTypes.Contains(ShapeType.Symbols) || _options.ShapeTypes.Contains(ShapeType.Rotated_Symbols))
                && _options.ShapeSymbolsToUse.Count == 0)
            {
                return;
            }

            Model geometrize = new(bitmap, commonColor.ToRgba32());

            for (int count = 0; count < _options.MaxSymbolCount; count++)
            {
                token.ThrowIfCancellationRequested();

                if (_options.IncludeBackground && count == 0)
                {
                    // background is always 100% opaque
                    layerDone.Invoke(ToLayer(
                        new ShapeAddResult()
                        {
                            Shape = new Geometrize.Shape.Rectangle(_originalImage.Width, _originalImage.Height)
                            {
                                x1 = 0,
                                y1 = 0,
                                x2 = _originalImage.Width,
                                y2 = _originalImage.Height,
                            },
                            Color = commonColor.ToRgba32(),
                            Score = -1,
                        }, 1));
                }
                else
                {
                    var opacity = count < _options.InitialShapeCountWithFullOpacity ? 1 : _options.SymbolOpacity;

                    var shape = geometrize.Step(
                        (int)Math.Round(opacity * 255),
                        _options.ShapesPerStep,
                        _options.MutationsPerStep,
                        symbolOptions,
                        token);

                    layerDone.Invoke(ToLayer(shape, opacity));

                    if (inProgressImageCallback != null)
                    {
                        inProgressImageCallback.Invoke(Image.LoadPixelData<Rgba32>(geometrize.current.data, bitmap.width, bitmap.height));
                    }
                }

                progress?.Report(new GeometrizeProgress() { Percentage = (double)count / _options.MaxSymbolCount * 100, Score = geometrize.score });
            }
        }

        private Abgr32[] IntToByteArray(int[] image)
        {
            Abgr32[] result = new Abgr32[image.Length];

            unchecked
            {
                for (int i = 0; i < image.Length; i++)
                {
                    result[i] = new Abgr32((uint)image[i]);
                }
            }

            return result;
        }

        private SymbolArtLayer ToLayer(ShapeAddResult shape, double opacity)
        {
            System.Windows.Point[] vertices;
            int symbolId;

            if (shape.Shape is Circle circle)
            {
                (symbolId, vertices) = ConvertCircle(circle);
            }
            else if (shape.Shape is Ellipse ellipse)
            {
                (symbolId, vertices) = ConvertEllipse(ellipse);
            }
            else if (shape.Shape is Geometrize.Shape.Rectangle rectangle)
            {
                (symbolId, vertices) = ConvertRectangle(rectangle);
            }
            else if (shape.Shape is SymbolShape symbol)
            {
                (symbolId, vertices) = ConvertSymbol(symbol);
            }
            else
            {
                throw new InvalidOperationException("Unsupported shape type");
            }

            return new SymbolArtLayer()
            {
                Alpha = opacity,
                Color = SymbolArtColorHelper.RemoveCurve(shape.Color.ToMediaColor()),
                Visible = true,
                SymbolId = symbolId,
                Vertex1 = vertices[0],
                Vertex2 = vertices[1],
                Vertex3 = vertices[2],
                Vertex4 = vertices[3],
            };
        }

        private (int SymbolId, System.Windows.Point[] Vertices) ConvertCircle(Circle shape)
        {
            double x = shape.x, y = shape.y, rx = shape.rx;

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

        private (int SymbolId, System.Windows.Point[] Vertices) ConvertEllipse(Ellipse shape)
        {
            double centerX = shape.x, centerY = shape.y, radiusX = shape.rx, radiusY = shape.ry;

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

            if (shape is RotatedEllipse rotatedEllipse)
                vertices = SymbolManipulationHelper.Rotate(vertices, (double)rotatedEllipse.angle / 180 * Math.PI);

            return (240, vertices);
        }

        private (int SymbolId, System.Windows.Point[] Vertices) ConvertSymbol(SymbolShape shape)
        {
            double x1 = shape.x1, y1 = shape.y1, x2 = shape.x2, y2 = shape.y2;

            int symbolId = shape.symbol.SymbolId;

            x1 *= _scale;
            y1 *= _scale;
            x2 *= _scale;
            y2 *= _scale;

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

            if (shape.flipX)
                vertices = SymbolManipulationHelper.FlipX(vertices);

            if (shape is RotatedSymbolShape rotatedSymbol)
                vertices = SymbolManipulationHelper.Rotate(vertices, (double)rotatedSymbol.angle / 180 * Math.PI);
            else if (shape.flipY)
                vertices = SymbolManipulationHelper.FlipY(vertices);

            return (symbolId - 1, vertices);
        }

        private (int SymbolId, System.Windows.Point[] Vertices) ConvertRectangle(Geometrize.Shape.Rectangle shape)
        {
            double x1 = shape.x1, y1 = shape.y1, x2 = shape.x2, y2 = shape.y2;

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

            if (shape is RotatedRectangle rotatedRect)
                vertices = SymbolManipulationHelper.Rotate(vertices, (double)rotatedRect.angle / 180 * Math.PI);

            return (242, vertices);
        }

        public void Dispose()
        {
            _originalImage.Dispose();
        }
    }

    public class GeometrizeProgress
    {
        public double Score { get; set; }

        public double Percentage { get; set; }
    }
}
