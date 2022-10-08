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

        public static SymbolArtGroup BitmapToSymbolArt(string filename, BitmapToSymbolArtConverterOptions? options = null)
        {
            if (options == null)
            {
                options = new();
            }

            using var image = Image.Load<Rgba32>(filename);

            image.Mutate(x => 
                x.Resize(0, options.ResizeImageHeight)
                .BackgroundColor(Color.White)
                .Quantize(new OctreeQuantizer(new QuantizerOptions() { MaxColors = options.MaxColors, Dither = null }))
            );

            var rootGroup = new SymbolArtGroup()
            {
                Visible = true,
                Name = $"{Path.GetFileName(filename)} converted"
            };

            var colors = SplitToColors(image).OrderByDescending(x => x.Pixels.Count).ToList();

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
                double extraWidth = ps.Width * pixelSize * (1 - options.SizeOffset);
                double extraHeight = ps.Height * pixelSize * (1 - options.SizeOffset);

                double left = ps.X * pixelSize - (image.Width * pixelSize / 2) - extraWidth / 2;
                double top = ps.Y * pixelSize - (image.Height * pixelSize / 2) - extraHeight / 2;
                double right = left + (ps.Width * pixelSize) + extraWidth;
                double bottom = top + (ps.Height * pixelSize) + extraHeight;

                rootGroup.Children.Add(new SymbolArtLayer()
                {
                    Color = SymbolArtColorHelper.RemoveCurve(System.Windows.Media.Color.FromRgb(ps.Color.R, ps.Color.G, ps.Color.B)),
                    SymbolId = ps.Width > 1 || ps.Height > 1 ? 242 : 246,
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
