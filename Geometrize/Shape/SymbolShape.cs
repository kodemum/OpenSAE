using Geometrize.Rasterizer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geometrize.Shape
{

#pragma warning disable 109, 114, 219, 429, 168, 162
    public class SymbolShape : IShape
    {
        protected readonly SymbolShapeOptions _symbolOptions;

        public SymbolShape()
        {

        }

        public SymbolShape(int xBound, int yBound, SymbolShapeOptions symbolOptions)
        {
            _symbolOptions = symbolOptions;

            var x = Std.random(xBound);
            var y = Std.random(yBound);
            var rx = Std.random(32) + 1;
            var ry = Std.random(32) + 1;

            x1 = x - rx;
            x2 = x + rx;
            y1 = y - ry;
            y2 = y + ry;

            this.xBound = xBound;
            this.yBound = yBound;

            if (_symbolOptions.SymbolDefinitions.Count == 0)
                throw new Exception("No registered symbols");

            symbol = _symbolOptions.SymbolDefinitions[Std.random(_symbolOptions.SymbolDefinitions.Count)];
            flipX = Std.random(2) != 0;
            flipY = Std.random(2) != 0;
        }

        public int x1;

        public int y1;

        public int x2;

        public int y2;

        public int xBound;

        public int yBound;

        public SymbolShapeDefinition symbol;

        public bool flipX;

        public bool flipY;

        public virtual IReadOnlyList<Scanline> Rasterize()
        {
            var lines = new List<Scanline>();

            int height = y2 > y1 ? y2 - y1 : y1 - y2;
            int width = x2 > x1 ? x2 - x1 : x1 - x2;

            double symbolScaleFactorY = 63d / height;
            double symbolScaleFactorX = 63d / width;

            if (x1 != x2 && y1 != y2)
            {
                for (int y = y1; y <= y2; y++)
                {
                    int? startX = null, endX = null;

                    int symbolY = (int)Math.Round((y - y1) * symbolScaleFactorY);

                    if (flipY)
                        symbolY = 63 - symbolY;

                    for (int x = x1; x <= x2; x++)
                    {
                        int symbolX = (int)Math.Round((x - x1) * symbolScaleFactorX);

                        if (flipX)
                            symbolX = 63 - symbolX;

                        if (symbol.SymbolScanlines[symbolY, symbolX] > 192)
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

                    if (startX != null && endX != null && y > 0 && y < yBound)
                        lines.Add(new Scanline(y, Math.Min(startX.Value, xBound - 1), Math.Min(endX.Value, xBound - 1)));
                }
            }

            return lines;
        }

        public virtual void Mutate()
        {
            unchecked
            {
                int r = Std.random(2);
                switch (r)
                {
                    case 0:
                        {
                            int @value = (this.x1 + ((-16 + ((int)(global::System.Math.Floor(((double)((33 * global::HaxeMath.rand.NextDouble())))))))));
                            int max = (this.xBound - 1);
                            if ((0 > max))
                            {
                                throw new Exception("FAIL: min <= max");
                            }

                            this.x1 = (((@value < 0)) ? (0) : ((((@value > max)) ? (max) : (@value))));
                            int value1 = (this.y1 + ((-16 + ((int)(global::System.Math.Floor(((double)((33 * global::HaxeMath.rand.NextDouble())))))))));
                            int max1 = (this.yBound - 1);
                            if ((0 > max1))
                            {
                                throw new Exception("FAIL: min <= max");
                            }

                            this.y1 = (((value1 < 0)) ? (0) : ((((value1 > max1)) ? (max1) : (value1))));
                            break;
                        }


                    case 1:
                        {
                            int value2 = (this.x2 + ((-16 + ((int)(global::System.Math.Floor(((double)((33 * global::HaxeMath.rand.NextDouble())))))))));
                            int max2 = (this.xBound - 1);
                            if ((0 > max2))
                            {
                                throw new Exception("FAIL: min <= max");
                            }

                            this.x2 = (((value2 < 0)) ? (0) : ((((value2 > max2)) ? (max2) : (value2))));
                            int value3 = (this.y2 + ((-16 + ((int)(global::System.Math.Floor(((double)((33 * global::HaxeMath.rand.NextDouble())))))))));
                            int max3 = (this.yBound - 1);
                            if ((0 > max3))
                            {
                                throw new Exception("FAIL: min <= max");
                            }

                            this.y2 = (((value3 < 0)) ? (0) : ((((value3 > max3)) ? (max3) : (value3))));
                            break;
                        }

                    case 2:
                        {
                            symbol = _symbolOptions.SymbolDefinitions[Std.random(_symbolOptions.SymbolDefinitions.Count)];
                        }
                        break;
                }

            }
        }


        public virtual IShape Clone()
        {
            return new SymbolShape(this.xBound, this.yBound, _symbolOptions)
            {
                x1 = this.x1,
                x2 = this.x2,
                y1 = this.y1,
                y2 = this.y2,
                symbol = this.symbol,
                flipX = this.flipX,
                flipY = this.flipY,
            };
        }

        public virtual double[] GetRawShapeData()
        {
            return new double[] {
                (this.x1 < this.x2) ? this.x1 : this.x2,
                (this.y1 < this.y2) ? this.y1 : this.y2,
                (this.x1 > this.x2) ? this.x1 : this.x2,
                (this.y1 > this.y2) ? this.y1 : this.y2,
                this.symbol.SymbolId,
                this.flipX ? 1 : 0,
                this.flipY ? 1 : 0,
            };
        }
    }
}
