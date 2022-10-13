using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Core.BitmapConverter
{
    public class BitmapToSymbolArtConverter
    {
        private static IEnumerable<PixelColor> SplitToColors(Image<Rgba32> image)
        {
            Dictionary<Rgba32, PixelColor> colors = new();

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var color = image[x, y];

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

            return colors.Values;
        }

        public static SymbolArtGroup ConvertSimple(SymbolArtGroup rootGroup, Image<Rgba32> image, BitmapToSymbolArtConverterOptions options)
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

        public static SymbolArtGroup BitmapToSymbolArt(string filename, BitmapToSymbolArtConverterOptions? options = null)
        {
            if (options == null)
            {
                options = new();
            }

            using var image = Image.Load<Rgba32>(filename);

            image.Mutate(x => x
                .Resize(0, options.ResizeImageHeight, options.SmoothResizing ? KnownResamplers.Lanczos8 : KnownResamplers.NearestNeighbor)
                .BackgroundColor(Color.White)
                .Quantize(new OctreeQuantizer(new QuantizerOptions() { MaxColors = options.MaxColors, Dither = null }))
            );

            var rootGroup = new SymbolArtGroup()
            {
                Visible = true,
                Name = $"{Path.GetFileName(filename)} converted"
            };

            if (options.DisableLayering)
            {
                return ConvertSimple(rootGroup, image, options);
            }
            else
            {
                return ConvertLayered(rootGroup, image, options);
            }
        }

        public static SymbolArtGroup ConvertLayered(SymbolArtGroup rootGroup, Image<Rgba32> image, BitmapToSymbolArtConverterOptions options)
        { 
            var colors = SplitToColors(image).OrderByDescending(x => x.Pixels.Count).ToList();

            bool[,] globalUsed = new bool[image.Width, image.Height];

            foreach (var colorItem in colors)
            {
                // area is transparent - we consider white that since that is more or less the background
                // for SA's in-game
                if (options.RemoveWhite && colorItem.Color.R > 251 && colorItem.Color.G > 251 && colorItem.Color.B > 251)
                {
                    continue;
                }

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

                double pixelSize = 96.0 / image.Height;

                foreach (var ps in pss)
                {
                    if (ps.Width * ps.Height < options.SymbolSizeThreshold)
                        continue;

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

                    colorGroup.Children.Add(new SymbolArtLayer()
                    {
                        Color = SymbolArtColorHelper.RemoveCurve(color),
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

                foreach (var (X, Y) in colorItem.Pixels)
                {
                    // enabling this isn't useful but creates kind of expressionist interpretations
                    // if (pss.Any(x => x.X == X && x.Y == Y && x.Width * x.Height >= options.SymbolSizeThreshold))
                    globalUsed[X, Y] = true;
                }

                if (colorGroup.Children.Count > 0)
                {
                    rootGroup.Children.Add(colorGroup);
                }
            }

            rootGroup.Children.Reverse();

            return rootGroup;
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
