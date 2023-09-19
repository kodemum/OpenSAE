using Geometrize.Rasterizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Geometrize.Shape
{
#pragma warning disable 109, 114, 219, 429, 168, 162
    public class RotatedSymbolShape : SymbolShape
    {
        public RotatedSymbolShape()
        {

        }

        public RotatedSymbolShape(int xBound, int yBound, SymbolShapeOptions symbolOptions)
            : base(xBound, yBound, symbolOptions) 
        {
            angle = Std.random(359);
        }

        public int angle;

        public override IReadOnlyList<Scanline> Rasterize()
        {
            double rads = angle * HaxeMath.PI / 180.0;
            double c = Math.Cos(rads);
            double s = Math.Sin(rads);

            int xm1 = Math.Min(x1, x2);
            int xm2 = Math.Max(x1, x2);
            int ym1 = Math.Min(y1, y2);
            int ym2 = Math.Max(y1, y2);

            int width = xm2 - xm1;
            int height = ym2 - ym1;

            int cx = (xm1 + xm2) / 2;
            int cy = (ym1 + ym2) / 2;

            int ox1 = xm1 - cx;
            int ox2 = xm2 - cx;
            int oy1 = ym1 - cy;
            int oy2 = ym2 - cy;

            int ulx = (int)((ox1 * c) - (oy1 * s) + cx);
            int uly = (int)((ox1 * s) + (oy1 * c) + cy);
            int blx = (int)((ox1 * c) - (oy2 * s) + cx);
            int bly = (int)((ox1 * s) + (oy2 * c) + cy);
            int urx = (int)((ox2 * c) - (oy1 * s) + cx);
            int ury = (int)((ox2 * s) + (oy1 * c) + cy);
            int brx = (int)((ox2 * c) - (oy2 * s) + cx);
            int bry = (int)((ox2 * s) + (oy2 * c) + cy);

            int minY = Math.Min(uly, Math.Min(bly, Math.Min(ury, bry)));
            int minX = Math.Min(ulx, Math.Min(blx, Math.Min(urx, brx)));
            int maxY = Math.Max(uly, Math.Max(bly, Math.Max(ury, bry)));
            int maxX = Math.Max(ulx, Math.Max(blx, Math.Max(urx, brx)));

            var lines = new List<Scanline>();

            double symbolScaleFactorY = 63d / height;
            double symbolScaleFactorX = 63d / width;

            for (int y = minY; y <= maxY; y++)
            {
                int? startX = null, endX = null;

                for (int x = minX; x < maxX; x++)
                {
                    int nx = (int)Math.Round((x - cx) * c + (y - cy) * s);
                    int ny = (int)Math.Round((x - cx) * -s + (y - cy) * c);

                    nx += width / 2;
                    ny += height / 2;

                    //Console.WriteLine("{0},{1} => {2},{3}", x, y, nx, ny);

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        int scaledX = (int)Math.Floor(nx * symbolScaleFactorX);
                        int scaledY = (int)Math.Floor(ny * symbolScaleFactorY);

                        if (flipX)
                        {
                            scaledX = 63 - scaledX;
                        }

                        if (symbol.SymbolScanlines[scaledY, scaledX] > 192)
                        {
                            if (startX == null)
                            {
                                startX = x;
                                endX = x;
                            }
                            else
                            {
                                endX = x;
                            }
                        }
                        else
                        {
                            if (startX != null && endX != null && y > 0 && y < yBound)
                            {
                                lines.Add(new Scanline(y, Math.Min(startX.Value, xBound - 1), Math.Min(endX.Value, xBound - 1)));
                                startX = null;
                                endX = null;
                            }
                        }
                    }
                    else
                    {
                        if (startX != null)
                        {
                            break;
                        }
                    }
                }

                if (startX != null && endX != null && y > 0 && y < yBound)
                    lines.Add(new Scanline(y, Math.Min(startX.Value, xBound - 1), Math.Min(endX.Value, xBound - 1)));
            }

            return lines;
        }

        public override void Mutate()
        {
            switch (HaxeMath.rand.Next(4))
            {
                case 0:
                    int centerX = (x1 + x2) / 2;
                    int centerY = (y1 + y2) / 2;
                    int newCenterX = Math.Clamp(centerX + -16 + HaxeMath.rand.Next(33), 0, xBound - 1);
                    int newCenterY = Math.Clamp(centerY + -16 + HaxeMath.rand.Next(33), 0, xBound - 1);

                    x1 += newCenterX - centerX;
                    x2 += newCenterX - centerX;
                    y1 += newCenterY - centerY;
                    y2 += newCenterY - centerY;

                    break;

                case 1:
                    int width = Math.Abs(x2 - x1) - 32 + HaxeMath.rand.Next(66);

                    if (x2 > x1)
                        x2 = Math.Clamp(x1 + width, 1, xBound - 1);
                    else
                        x2 = Math.Clamp(x2 + width, 1, xBound - 1);
                    break;

                case 2:
                    int height = Math.Abs(y2 - y1);

                    height = Math.Clamp(height - 32 + HaxeMath.rand.Next(66), 1, xBound - 1);

                    if (y2 > y1)
                        y2 = y1 + height;
                    else
                        y1 = y2 + height;
                    break;

                case 3:
                    angle = Math.Clamp(angle - 4 + HaxeMath.rand.Next(9), 0, 180);
                    break;

                case 4:
                    flipX = !flipX;
                    break;
            }

            if (x1 == x2 || y1 == y2)
            {
                x1 = Math.Clamp(x1 + -16 + HaxeMath.rand.Next(33), 0, xBound - 1);
                y2 = Math.Clamp(y2 + -16 + HaxeMath.rand.Next(33), 0, xBound - 1);
            }
        }


        public override IShape Clone()
        {
            return new RotatedSymbolShape(xBound, yBound, _symbolOptions)
            {
                x1 = x1,
                x2 = x2,
                y1 = y1,
                y2 = y2,
                angle = angle,
                symbol = symbol,
                flipX = flipX,
            };
        }

        public override double[] GetRawShapeData()
        {
            return new double[] {
                (x1 < x2) ? x1 : x2,
                (y1 < y2) ? y1 : y2,
                (x1 > x2) ? x1 : x2,
                (y1 > y2) ? y1 : y2,
                symbol.SymbolId,
                angle,
                flipX ? 1 : 0
            };
        }
    }
}
