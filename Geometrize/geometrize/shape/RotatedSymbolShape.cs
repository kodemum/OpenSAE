using geometrize.rasterizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace geometrize.shape
{
#pragma warning disable 109, 114, 219, 429, 168, 162
    public class RotatedSymbolShape : haxe.lang.HxObject, Shape
    {
        private readonly SymbolShapeOptions _symbolOptions;

        public RotatedSymbolShape()
        {

        }

        public RotatedSymbolShape(int xBound, int yBound, SymbolShapeOptions symbolOptions)
        {
            _symbolOptions = symbolOptions;
            int x = Std.random(xBound);
            int y = Std.random(yBound);
            int width = Std.random(64) + 1;
            int height = Std.random(64) + 1;

            x1 = Math.Clamp(x - (width / 2), 0, xBound - 1);
            y1 = Math.Clamp(y - (height / 2), 0, yBound - 1);
            x2 = Math.Clamp(x + (width / 2), 0, xBound - 1);
            y2 = Math.Clamp(y + (height / 2), 0, yBound - 1);

            this.xBound = xBound;
            this.yBound = yBound;

            if (_symbolOptions.SymbolDefinitions.Count == 0)
                throw new Exception("No registered symbols");

            symbol = _symbolOptions.SymbolDefinitions[Std.random(_symbolOptions.SymbolDefinitions.Count)];
            angle = Std.random(359);
        }

        public int x1;

        public int y1;

        public int x2;

        public int y2;

        public int angle;

        public int xBound;

        public int yBound;

        private SymbolShapeDefinition symbol;

        public HaxeArray<object> rasterize()
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

            HaxeArray<object> lines = new HaxeArray<object>(new object[] { });

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

                        var scanLine = symbol.SymbolScanlines[scaledY];

                        if (scanLine.x <= scaledX && scanLine.x2 >= scaledX)
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
                    lines.push(new Scanline(y, Math.Min(startX.Value, xBound - 1), Math.Min(endX.Value, xBound - 1)));
            }

            return lines;
        }

        public virtual void mutate()
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

                //case 3:
                //    symbol = symbols[Std.random(symbols.Count)];
                //    break;

                case 3:
                    angle = Math.Clamp(angle - 4 + HaxeMath.rand.Next(9), 0, 259);
                    break;
            }

            if (x1 == x2 || y1 == y2)
            {
                x1 = Math.Clamp(x1 + -16 + HaxeMath.rand.Next(33), 0, xBound - 1);
                y2 = Math.Clamp(y2 + -16 + HaxeMath.rand.Next(33), 0, xBound - 1);
            }
        }


        public virtual Shape clone()
        {
            return new RotatedSymbolShape(xBound, yBound, _symbolOptions)
            {
                x1 = x1,
                x2 = x2,
                y1 = y1,
                y2 = y2,
                angle = angle,
                symbol = symbol,
            };
        }


        public virtual int getType()
        {
            return 8;
        }


        public virtual HaxeArray<double> getRawShapeData()
        {
            return new HaxeArray<double>(new double[] {
                (x1 < x2) ? x1 : x2,
                (y1 < y2) ? y1 : y2,
                (x1 > x2) ? x1 : x2,
                (y1 > y2) ? y1 : y2,
                symbol.SymbolId,
                angle,
            });
        }


        public virtual string getSvgShapeData()
        {
            int first = x1;
            int second = x2;
            int first1 = y1;
            int second1 = y2;
            int first2 = x1;
            int second2 = x2;
            int first3 = x1;
            int second3 = x2;
            int first4 = y1;
            int second4 = y2;
            int first5 = y1;
            int second5 = y2;
            return haxe.lang.Runtime.concat(haxe.lang.Runtime.concat(haxe.lang.Runtime.concat(haxe.lang.Runtime.concat(haxe.lang.Runtime.concat(haxe.lang.Runtime.concat(haxe.lang.Runtime.concat(haxe.lang.Runtime.concat(haxe.lang.Runtime.concat(haxe.lang.Runtime.concat("<rect x=\"", haxe.lang.Runtime.toString((first < second) ? first : second)), "\" y=\""), haxe.lang.Runtime.toString((first1 < second1) ? first1 : second1)), "\" width=\""), haxe.lang.Runtime.toString(((first2 > second2) ? first2 : second2) - ((first3 < second3) ? first3 : second3))), "\" height=\""), haxe.lang.Runtime.toString(((first4 > second4) ? first4 : second4) - ((first5 < second5) ? first5 : second5))), "\" "), exporter.SvgExporter.SVG_STYLE_HOOK), " />");
        }
    }
}
