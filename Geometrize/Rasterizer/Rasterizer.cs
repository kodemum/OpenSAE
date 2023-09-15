// Generated by Haxe 4.3.1

using System;
using System.Collections.Generic;

namespace Geometrize.Rasterizer
{
    public static class Rasterizer
    {

        public static void DrawLines(Bitmap image, int color, IEnumerable<Scanline> lines)
        {
            unchecked
            {
                if (image == null)
                {
                    throw new Exception("FAIL: image != null");
                }

                if (lines == null)
                {
                    throw new Exception("FAIL: lines != null");
                }

                int sr = (color >> 24) & 255;
                sr |= sr << 8;
                sr *= color & 255;
                sr /= 255;

                int sg = (color >> 16) & 255;
                sg |= sg << 8;
                sg *= color & 255;
                sg /= 255;

                int sb = (color >> 8) & 255;
                sb |= sb << 8;
                sb *= color & 255;
                sb /= 255;

                int sa = color & 255;
                sa |= sa << 8;

                foreach (var line in lines)
                {
                    int y = line.y;
                    int ma = 65535;
                    int m = 65535;
                    int a = (int)(double)((m - (sa * (((double)ma) / m))) * 257);

                    for (int x = line.x1; x < line.x2 + 1; x++)
                    {
                        if (line.transparencyData != null)
                        {
                            sa = line.transparencyData[x - line.x1];

                            int colorWithModeratedTransparency = color - (color & 255) + sa;

                            image.data[(image.width * y) + x] = Bitmap.Add(colorWithModeratedTransparency, image.data[(image.width * y) + x]);
                            continue;
                        }

                        int d = image.data[(image.width * y) + x];
                        int dr = (d >> 24) & 255;
                        int dg = (d >> 16) & 255;
                        int db = (d >> 8) & 255;
                        int da = d & 255;
                        byte r = (byte)(((int)(((double)((dr * a) + (sr * ma))) / m)) >> 8);
                        byte g = (byte)(((int)(((double)((dg * a) + (sg * ma))) / m)) >> 8);
                        byte b = (byte)(((int)(((double)((db * a) + (sb * ma))) / m)) >> 8);
                        byte a1 = (byte)(((int)(((double)((da * a) + (sa * ma))) / m)) >> 8);

                        image.data[(image.width * y) + x] = 
                            (((r < 0) ? 0 : ((r > 255) ? 255 : ((int)r))) << 24) + 
                            (((g < 0) ? 0 : ((g > 255) ? 255 : ((int)g))) << 16) + 
                            (((b < 0) ? 0 : ((b > 255) ? 255 : ((int)b))) << 8) + 
                            ((a1 < 0) ? 0 : ((a1 > 255) ? 255 : ((int)a1)));
                    }
                }
            }
        }

        public static void CopyLines(Bitmap destination, Bitmap source, IEnumerable<Scanline> lines)
        {
            unchecked
            {
                if (destination == null)
                {
                    throw new Exception("FAIL: destination != null");
                }

                if (source == null)
                {
                    throw new Exception("FAIL: source != null");
                }

                if (lines == null)
                {
                    throw new Exception("FAIL: lines != null");
                }

                foreach (var line in lines)
                {
                    for (int x = line.x1; x < line.x2 + 1; x++)
                    {
                        destination.data[(destination.width * line.y) + x] = source.data[(source.width * line.y) + x];
                    }
                }
            }
        }


        public static List<Point> Bresenham(int x1, int y1, int x2, int y2)
        {
            unchecked
            {
                int dx = x2 - x1;
                int ix = ((dx > 0) ? 1 : 0) - ((dx < 0) ? 1 : 0);
                dx = ((dx < 0) ? (-dx) : dx) << 1;
                int dy = y2 - y1;
                int iy = ((dy > 0) ? 1 : 0) - ((dy < 0) ? 1 : 0);
                dy = ((dy < 0) ? (-dy) : dy) << 1;

                var points = new List<Point>
                {
                    new Point(x1, y1)
                };

                if (dx >= dy)
                {
                    int error = dy - (dx >> 1);
                    while (x1 != x2)
                    {
                        if ((error >= 0) && ((error != 0) || (ix > 0)))
                        {
                            error -= dx;
                            y1 += iy;
                        }

                        error += dy;
                        x1 += ix;

                        points.Add(new Point(x1, y1));
                    }

                }
                else
                {
                    int error1 = dx - (dy >> 1);
                    while (y1 != y2)
                    {
                        if ((error1 >= 0) && ((error1 != 0) || (iy > 0)))
                        {
                            error1 -= dy;
                            x1 += ix;
                        }

                        error1 += dx;
                        y1 += iy;

                        points.Add(new Point(x1, y1));
                    }

                }

                return points;
            }
        }


        public static List<Scanline> ScanlinesForPolygon(IReadOnlyList<Point> points)
        {
            var lines = new List<Scanline>();
            var edges = new List<Point>();

            for (int i = 0; i < points.Count; i++)
            {
                var p1 = points[i];
                var p2 = (i == points.Count - 1) ? points[0] : points[i + 1];
                var p1p2 = Bresenham(p1.X, p1.Y, p2.X, p2.Y);

                edges.AddRange(p1p2);
            }

            var yToXs = new Dictionary<int, List<int>>();
            foreach (var point in edges)
            {
                if (!yToXs.TryGetValue(point.Y, out var xList))
                {
                    xList = new List<int>();
                    yToXs[point.Y] = xList;
                }

                xList.Add(point.X);
            }

            foreach (int y in yToXs.Keys)
            {
                (int min, int max) = Util.GetMinMax(yToXs[y]);

                lines.Add(new Scanline(y, min, max));
            }

            return lines;
        }
    }
}


