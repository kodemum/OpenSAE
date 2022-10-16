using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace OpenSAE.Core.BitmapConverter
{
    public class BitmapToSymbolArtConverter : IDisposable
    {
        public string Filename { get; }

        private readonly BitmapToSymbolArtConverterOptions options;

        private readonly Image<Rgba32> originalImage;
        private readonly Image<Rgba32> image;

        private readonly double pixelSize;

        /// <summary>
        /// Array of symbols usable for the converter
        /// </summary>
        private static readonly Symbol[] usableSymbols = SymbolUtil.List.Where(x =>
            x.Group == SymbolGroup.FilledSymbols || x.Group == SymbolGroup.LineSymbols || x.Id == 681).ToArray();

        /// <summary>
        /// Array of arrays containing extent info for the usable symbols
        /// </summary>
        private static readonly bool[][] symbolExtents;

        /// <summary>
        /// Actual (cropped) symbol height/width
        /// </summary>
        private static readonly int actualSymbolWidth;

        private static bool debug = false;
        private readonly string _debugDirectory;

        public Action<double>? ConvertProgress { get; set; }

        static BitmapToSymbolArtConverter()
        {
            // create arrays for all usable symbols that only contain their alpha channel - since this is all we care about
            int actualWidth = (int)Math.Round(64 * 0.8);
            symbolExtents = new bool[usableSymbols.Length][];

            for (int symbolId = 0; symbolId < usableSymbols.Length; symbolId++)
            {
                byte[] pixelValues = new byte[4 * actualWidth * actualWidth];

                // crop the symbol and copy the pixel data
                var rect = new Int32Rect((64 - actualWidth) / 2, (64 - actualWidth) / 2, actualWidth, actualWidth);
                usableSymbols[symbolId].Image.CopyPixels(rect, pixelValues, 4 * actualWidth, 0);

                symbolExtents[symbolId] = new bool[actualWidth * actualWidth];

                for (int i = 0; i < actualWidth * actualWidth; i++)
                {
                    // set each pixel if the alpha is higher than the set threshold
                    symbolExtents[symbolId][i] = pixelValues[i * 4 + 3] > 40;
                }
            }

            actualSymbolWidth = actualWidth;
        }

        public BitmapToSymbolArtConverter(string filename, BitmapToSymbolArtConverterOptions? op = null)
        {
            Filename = filename;
            options = op ?? new BitmapToSymbolArtConverterOptions();

            originalImage = Image.Load<Rgba32>(filename);
            originalImage.Mutate(x => x.BackgroundColor(Color.White));

            image = originalImage.Clone();

            image.Mutate(x =>
                x.Resize(0, options.ResizeImageHeight, options.SmoothResizing ? KnownResamplers.Lanczos8 : KnownResamplers.NearestNeighbor)
                .Quantize(new OctreeQuantizer(new QuantizerOptions() { MaxColors = options.MaxColors, Dither = null }))
                );

            originalImage.Mutate(x => x.Quantize(new OctreeQuantizer(new QuantizerOptions() { MaxColors = options.MaxColors, Dither = null })));

            pixelSize = 96.0 / image.Height;

            _debugDirectory = Path.Combine("converter-debug", Path.GetFileNameWithoutExtension(filename));

            if (debug)
            {
                Directory.CreateDirectory(_debugDirectory);
            }
        }

        private static IEnumerable<PixelColor> SplitToColors(Image<Rgba32> image)
        {
            Dictionary<Rgba32, PixelColor> colors = new();

            image.ProcessPixelRows(rows =>
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var row = rows.GetRowSpan(y);

                    for (int x = 0; x < image.Width; x++)
                    {
                        var color = row[x];

                        if (colors.TryGetValue(color, out PixelColor? pixel))
                        {
                            pixel.Pixels.Add((x, y));
                        }
                        else
                        {
                            colors.Add(color, new PixelColor(color, x, y));
                        }
                    }
                }
            });

            return colors.Values;
        }

        public SymbolArtGroup ConvertSimple(SymbolArtGroup rootGroup)
        {
            bool[,] filled = new bool[image.Width, image.Height];

            List<PseudoPixel> pss = new();

            PseudoPixel? current = null;

            void ExpandX(int x, PseudoPixel current)
            {
                int expandBy = 0;
                bool running = true;

                while (running && x + expandBy < image.Width - 1)
                {
                    expandBy++;

                    for (int y2 = current.Y; y2 < Math.Min(current.Y + current.Height, image.Height); y2++)
                    {
                        if (image[x + expandBy, y2] != current.Color)
                        {
                            expandBy--;
                            running = false;
                            break;
                        }
                    }
                }

                if (expandBy > 0)
                {
                    current.Width += expandBy;
                }

                for (int cx = current.X; cx < current.X + current.Width; cx++)
                    for (int cy = current.Y; cy < current.Y + current.Height; cy++)
                    {
                        filled[cx, cy] = true;
                    }
            }

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (current != null && !filled[x, y] && image[x, y] == current.Color)
                    {
                        // current color matches current square - see if it can be expanded
                        current.Height++;
                    }

                    if (!filled[x, y])
                    {
                        if (current != null)
                        {
                            ExpandX(x, current);
                        }

                        current = new PseudoPixel()
                        {
                            X = x,
                            Y = y,
                            Width = 1,
                            Height = 1,
                            Color = image[x, y]
                        };

                        pss.Add(current);
                    }
                }

                if (current != null)
                {
                    ExpandX(x, current);
                    current = null;
                }
            }

            double pixelSize = 96.0 / image.Height;

            foreach (var ps in pss)
            {
                // area is transparent - we consider white that since that is more or less the background
                // for SA's in-game
                if (options.RemoveWhite && ps.Color.R > 251 && ps.Color.G > 251 && ps.Color.B > 251)
                {
                    continue;
                }

                // since symbols do not take up the entirety of the area defined by their vertices, we
                // have to take this into account when trying to make a grid of them
                double extraWidth = ps.Width * pixelSize * (1 - 0.65);
                double extraHeight = ps.Height * pixelSize * (1 - 0.65);

                double left = ps.X * pixelSize - (image.Width * pixelSize / 2) - extraWidth / 2;
                double top = ps.Y * pixelSize - (image.Height * pixelSize / 2) - extraHeight / 2;
                double right = left + (ps.Width * pixelSize) + extraWidth;
                double bottom = top + (ps.Height * pixelSize) + extraHeight;

                rootGroup.Children.Add(new SymbolArtLayer()
                {
                    Color = SymbolArtColorHelper.RemoveCurve(System.Windows.Media.Color.FromRgb(ps.Color.R, ps.Color.G, ps.Color.B)),
                    SymbolId = options.PixelSymbol,
                    Vertex1 = new System.Windows.Point(left, top),
                    Vertex2 = new System.Windows.Point(left, bottom),
                    Vertex3 = new System.Windows.Point(right, bottom),
                    Vertex4 = new System.Windows.Point(right, top),
                    Visible = true,
                    Name = $"{ps.X}/{ps.Y} {ps.Width}x{ps.Height}",
                    Alpha = ps.Color.A / 255.0
                });
            }

            return rootGroup;
        }

        public SymbolArtGroup Convert(CancellationToken token, IProgress<double>? progress = null)
        {
            var rootGroup = new SymbolArtGroup()
            {
                Visible = true,
                Name = $"{Path.GetFileName(Filename)} converted"
            };

            if (options.DisableLayering)
            {
                return ConvertSimple(rootGroup);
            }
            else
            {
                return ConvertLayered(rootGroup, token, progress);
            }
        }

        public SymbolArtGroup ConvertLayered(SymbolArtGroup rootGroup, CancellationToken token, IProgress<double>? progress = null)
        {
            var colors = SplitToColors(image).OrderByDescending(x => x.Pixels.Count).ToList();

            int totalPixels = image.Width * image.Height;
            int processedPixels = 0;

            bool[,] globalUsed = new bool[image.Width, image.Height];

            int colorCount = 0;

            foreach (var colorItem in colors)
            {
                var color = System.Windows.Media.Color.FromRgb(colorItem.Color.R, colorItem.Color.G, colorItem.Color.B);
                var colorGroup = new SymbolArtGroup()
                {
                    Visible = true,
                    Name = ColorNameMapper.GetNearestName(color)
                };

                bool[,] available = new bool[image.Width, image.Height];

                foreach (var (X, Y) in colorItem.Pixels)
                {
                    available[X, Y] = true;
                }

                List<PseudoPixel> pss = new();

                foreach (var (x, y) in colorItem.Pixels)
                {
                    if (!available[x, y])
                        continue;

                    PseudoPixel current = new()
                    {
                        X = x,
                        Y = y,
                        Width = 1,
                        Height = 1,
                        Color = image[x, y]
                    };

                    pss.Add(current);

                    bool isAvailable(int x, int y) => !globalUsed[x, y];

                    // while we can use both X and Y extent and choose the one that fits best
                    var extendX = FindLargestExtent(image, x, y, true, isAvailable, (x, y) => image[x, y] == colorItem.Color);
                    var extendY = FindLargestExtent(image, x, y, false, isAvailable, (x, y) => image[x, y] == colorItem.Color);
                    var extend = (extendX.y * extendX.x > extendY.y * extendY.x) ? extendX : extendY;

                    current.Width += extend.x;
                    current.Height += extend.y;

                    for (int cx = current.X; cx < current.X + current.Width; cx++)
                        for (int cy = current.Y; cy < current.Y + current.Height; cy++)
                        {
                            available[cx, cy] = false;
                        }
                }

                if (colorCount == 0)
                {
                    // first color considered background
                    pss.Clear();
                    pss.Add(new PseudoPixel()
                    {
                        Color = colorItem.Color,
                        X = 0,
                        Y = 0,
                        Width = image.Width,
                        Height = image.Height
                    });
                }

                int pixelsInColor = colorItem.Pixels.Count;

                for (int i = 0; i < pss.Count; i++)
                {
                    PseudoPixel? ps = pss[i];

                    if (ps.Width * ps.Height < options.SymbolSizeThreshold)
                        continue;

                    token.ThrowIfCancellationRequested();
                    progress?.Report((double)(processedPixels + (double)pixelsInColor / pss.Count * (i + 1)) / totalPixels * 100);

                    int symbolId;
                    int rotate = 0;

                    if (options.AutoChooseSymbols && colorCount > 0)
                    {
                        var symbol = FindBestOverlappingShape(ps);

                        if (symbol.symbol == null)
                            continue;

                        symbolId = symbol.symbol.Id - 1;
                        rotate = symbol.rotation;
                    }
                    else
                    {
                        symbolId = options.PixelSymbol;
                    }

                    // since symbols do not take up the entirety of the area defined by their vertices, we
                    // have to take this into account when trying to make a grid of them
                    double extraWidth = ps.Width * pixelSize * (1 - options.SizeXOffset);
                    double extraHeight = ps.Height * pixelSize * (1 - options.SizeYOffset);

                    extraWidth *= Math.Pow(ps.Width, options.OffsetSizeXExponent) / ps.Width;
                    extraHeight *= Math.Pow(ps.Height, options.OffsetSizeYExponent) / ps.Height;

                    double left = ps.X * pixelSize - (image.Width * pixelSize / 2) - extraWidth / 2;
                    double top = ps.Y * pixelSize - (image.Height * pixelSize / 2) - extraHeight / 2;
                    double right = left + (ps.Width * pixelSize) + extraWidth;
                    double bottom = top + (ps.Height * pixelSize) + extraHeight;

                    if (ps.Width > 1 || ps.Height > 1)
                    {
                        double positionYOffset = options.CenterYOffset * Math.Pow(ps.Height, options.OffsetSizeYExponent) / ps.Height;
                        double positionXOffset = options.CenterXOffset * Math.Pow(ps.Width, options.OffsetSizeXExponent) / ps.Width;

                        top -= positionYOffset;
                        bottom -= positionYOffset;
                        left -= positionXOffset;
                        right -= positionXOffset;
                    }

                    var vertices = new System.Windows.Point[]
                    {
                        new System.Windows.Point(left, top),
                        new System.Windows.Point(left, bottom),
                        new System.Windows.Point(right, bottom),
                        new System.Windows.Point(right, top),
                    };

                    if (rotate == 1)
                    {
                        var start = vertices[0];
                        vertices[0] = vertices[1];
                        vertices[1] = vertices[2];
                        vertices[2] = vertices[3];
                        vertices[3] = start;
                    }
                    else if (rotate == 2)
                    {
                        var zero = vertices[0];
                        var one = vertices[1];
                        vertices[0] = vertices[2];
                        vertices[1] = vertices[3];
                        vertices[2] = zero;
                        vertices[3] = one;
                    }
                    else if (rotate == 3)
                    {
                        var three = vertices[3];
                        vertices[3] = vertices[2];
                        vertices[2] = vertices[1];
                        vertices[1] = vertices[0];
                        vertices[0] = three;
                    }

                    colorGroup.Children.Add(new SymbolArtLayer()
                    {
                        Color = SymbolArtColorHelper.RemoveCurve(color),
                        SymbolId = symbolId,
                        Vertex1 = vertices[0],
                        Vertex2 = vertices[1],
                        Vertex3 = vertices[2],
                        Vertex4 = vertices[3],
                        Visible = true,
                        Name = $"{ps.X}/{ps.Y} {ps.Width}x{ps.Height}",
                        Alpha = ps.Color.A / 255.0
                    });
                }

                foreach (var (X, Y) in colorItem.Pixels)
                {
                    // enabling this isn't useful but creates kind of expressionist interpretations
                    //if (pss.Any(x => x.X == X && x.Y == Y && x.Width * x.Height >= options.SymbolSizeThreshold))
                    globalUsed[X, Y] = true;
                }

                processedPixels += pixelsInColor;

                // area is transparent - we consider white that since that is more or less the background
                // for SA's in-game
                if (colorGroup.Children.Count > 0 && !(options.RemoveWhite && colorItem.Color.R > 251 && colorItem.Color.G > 251 && colorItem.Color.B > 251))
                {
                    rootGroup.Children.Add(colorGroup);
                }

                colorCount++;
            }

            rootGroup.Children.Reverse();

            return rootGroup;
        }

        private (Symbol? symbol, int rotation) FindBestOverlappingShape(PseudoPixel ps)
        {
            double ratio = (double)originalImage.Height / image.Height;

            using var cut = originalImage.Clone();

            var cropBounds = new Rectangle((int)Math.Floor(ps.X * ratio), (int)Math.Floor(ps.Y * ratio), (int)Math.Floor(ps.Width * ratio), (int)Math.Floor(ps.Height * ratio));

            cut.Mutate(x =>
            {
                if (cropBounds.Width + cropBounds.X > cut.Width)
                {
                    cropBounds.Width = cut.Width - cropBounds.X - 1;
                }

                if (cropBounds.Height + cropBounds.Y > cut.Height)
                {
                    cropBounds.Height = cut.Height - cropBounds.Y - 1;
                }

                x.Crop(cropBounds).Resize(actualSymbolWidth, actualSymbolWidth, KnownResamplers.NearestNeighbor);
            });

            int targetCount = 0;

            if (debug)
            {
                cut.SaveAsPng(Path.Combine(_debugDirectory, $"{ps.X},{ps.Y}.png"));
            }

            List<(Symbol symbol, int rotation, int fit)> scores = new();

            cut.ProcessPixelRows(rows =>
            {
                for (int y = 0; y < actualSymbolWidth; y++)
                {
                    var span = rows.GetRowSpan(y);

                    for (int x = 0; x < actualSymbolWidth; x++)
                    {
                        if (CloseEnough(span[x], ps.Color))
                        {
                            targetCount++;
                        }
                    }
                }

                for (int i = 0; i < usableSymbols.Length; i++)
                {
                    Symbol symbol = usableSymbols[i];

                    bool[] pixelValues = symbolExtents[i];

                    for (int rotation = 0; rotation < 4; rotation++)
                    {
                        int match = 0;

                        if (symbol.Flags.HasFlag(SymbolFlag.Symmetric) && rotation > 0)
                            continue;

                        for (int y = 0; y < actualSymbolWidth; y++)
                        {
                            var span = rows.GetRowSpan(y);

                            for (int x = 0; x < actualSymbolWidth; x++)
                            {
                                if (CloseEnough(span[x], ps.Color))
                                {
                                    int xi = x, yi = y;

                                    switch (rotation)
                                    {
                                        case 1:
                                            yi = actualSymbolWidth - x - 1;
                                            xi = y;
                                            break;

                                        case 2:
                                            xi = actualSymbolWidth - x - 1;
                                            yi = actualSymbolWidth - y - 1;
                                            break;

                                        case 3:
                                            xi = actualSymbolWidth - y - 1;
                                            yi = x;
                                            break;
                                    }

                                    int index = (yi * actualSymbolWidth + xi);

                                    if (pixelValues[index])
                                    {
                                        match++;
                                    }
                                }
                            }
                        }

                        int fit = Math.Abs(match - targetCount);

                        scores.Add((symbol, rotation, fit));
                    }
                }
            });

            if (debug)
            {
                var debugScores = scores.OrderBy(x => x.fit).Select(x => new { symbol = x.symbol.Name, fit = x.fit, rotation = x.rotation }).ToArray();

                File.WriteAllText(Path.Combine(_debugDirectory, $"{ps.X},{ps.Y}.json"), JsonSerializer.Serialize(debugScores));
            }

            return scores.OrderBy(x => x.fit).Select(x => (x.symbol, x.rotation)).First();
        }

        private static bool CloseEnough(Rgba32 color1, Rgba32 color2)
        {
            return Math.Abs(color1.R - color2.R) + Math.Abs(color1.G - color2.G) + Math.Abs(color1.B - color2.B)
                < 8;
        }

        private static (int x, int y) FindLargestExtent(Image<Rgba32> image, int sourceX, int sourceY, bool xFirst, Func<int, int, bool> isAvailable, Func<int, int, bool> isColor)
        {
            int expandByX = 0, expandByY = 0;
            int lastColorX = -1, lastColorY = -1;
            bool atEnd = false;

            if (xFirst)
            {
                for (int x = sourceX; x < image.Width - 1; x++)
                {
                    if (!isAvailable(x + 1, sourceY))
                    {
                        break;
                    }

                    if (isColor(x + 1, sourceY))
                    {
                        lastColorX = x + 1;
                        lastColorY = sourceY;
                    }

                    expandByX++;
                }

                for (int y = sourceY; y < image.Height - 1; y++)
                {
                    for (int x = sourceX; x <= sourceX + expandByX; x++)
                    {
                        if (!isAvailable(x, y + 1))
                        {
                            atEnd = true;
                            break;
                        }
                        else if (isColor(x, y + 1))
                        {
                            lastColorX = x;
                            lastColorY = y + 1;
                        }
                    }

                    if (atEnd)
                        break;
                    else
                        expandByY++;
                }

                if (lastColorX > -1)
                {
                    return (lastColorX - sourceX, lastColorY - sourceY);
                }
                else
                {
                    return (0, 0);
                }
            }
            else
            {
                for (int y = sourceY; y < image.Height - 1; y++)
                {
                    if (!isAvailable(sourceX, y + 1))
                    {
                        break;
                    }

                    expandByY++;
                }

                for (int x = sourceX; x < image.Width - 1; x++)
                {
                    for (int y = sourceY; y <= sourceY + expandByY; y++)
                    {
                        if (!isAvailable(x + 1, y))
                        {
                            atEnd = true;
                            break;
                        }
                    }

                    if (atEnd)
                        break;
                    else
                        expandByX++;
                }
            }

            return (expandByX, expandByY);
        }

        public void Dispose()
        {
            image.Dispose();
            originalImage.Dispose();
        }

        private class PseudoPixel
        {
            public int Width;
            public int Height;

            public int X;
            public int Y;

            public Rgba32 Color;
        }
    }
}
