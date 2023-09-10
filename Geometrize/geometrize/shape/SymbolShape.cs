using System;
using System.Collections.Generic;
using System.Text;

namespace geometrize.shape
{
    public class SymbolShapeDefinition
    {
        public int SymbolId { get; set; }

        public (int x, int x2)[] SymbolScanlines { get; set; }
    }

#pragma warning disable 109, 114, 219, 429, 168, 162
    public class SymbolShape : haxe.lang.HxObject, Shape
    {
        private static readonly List<SymbolShapeDefinition> symbols = new List<SymbolShapeDefinition>();

        public static void RegisterSymbol(SymbolShapeDefinition definition)
        {
            symbols.Add(definition);
        }

        public SymbolShape()
        {

        }

        public SymbolShape(int xBound, int yBound)
        {
            x1 = Std.random(xBound);
            y1 = Std.random(yBound);
            int @value = ((x1 + Std.random(32)) + 1);
            int max = (xBound - 1);
            if ((0 > max))
            {
                throw ((Exception)(haxe.Exception.thrown("FAIL: min <= max")));
            }

            x2 = (@value < 0) ? 0 : ((@value > max) ? max : @value);
            int value1 = ((y1 + Std.random(32)) + 1);
            int max1 = (yBound - 1);
            if ((0 > max1))
            {
                throw ((Exception)(haxe.Exception.thrown("FAIL: min <= max")));
            }

            y2 = (value1 < 0) ? 0 : ((value1 > max1) ? max1 : value1);
            this.xBound = xBound;
            this.yBound = yBound;

            if (symbols.Count == 0)
                throw new Exception("No registered symbols");

            symbol = symbols[Std.random(symbols.Count)];
        }

        public int x1;

        public int y1;

        public int x2;

        public int y2;

        public int xBound;

        public int yBound;

        private SymbolShapeDefinition symbol;

        public virtual HaxeArray<object> rasterize()
        {
            HaxeArray<object> lines = new HaxeArray<object>(new object[] { });

            int height = y2 > y1 ? y2 - y1 : y1 - y2;
            int width = x2 > x1 ? x2 - x1 : x1 - x2;
            int start = x2 > x1 ? x1 : x2;

            double symbolScaleFactorY = 63d / height;
            double symbolScaleFactorX = 63d / width;

            if (x1 != x2 && y1 != y2)
            {
                for (int y = y1; y <= y2; y++)
                {
                    int symbolY = (int)Math.Round((y - y1) * symbolScaleFactorY);

                    (int symbolX, int symbolX2) = symbol.SymbolScanlines[symbolY];

                    if (symbolX != symbolX2)
                    {
                        int translatedSymbolX = (int)Math.Round(start + (symbolX / symbolScaleFactorX));
                        int translatedSymbolX2 = (int)Math.Round(start + (symbolX2 / symbolScaleFactorX));

                        lines.push(new rasterizer.Scanline(y, translatedSymbolX, translatedSymbolX2));
                    }
                }
            }

            return lines;
        }

        public virtual void mutate()
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
                                throw ((global::System.Exception)(global::haxe.Exception.thrown("FAIL: min <= max")));
                            }

                            this.x1 = (((@value < 0)) ? (0) : ((((@value > max)) ? (max) : (@value))));
                            int value1 = (this.y1 + ((-16 + ((int)(global::System.Math.Floor(((double)((33 * global::HaxeMath.rand.NextDouble())))))))));
                            int max1 = (this.yBound - 1);
                            if ((0 > max1))
                            {
                                throw ((global::System.Exception)(global::haxe.Exception.thrown("FAIL: min <= max")));
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
                                throw ((global::System.Exception)(global::haxe.Exception.thrown("FAIL: min <= max")));
                            }

                            this.x2 = (((value2 < 0)) ? (0) : ((((value2 > max2)) ? (max2) : (value2))));
                            int value3 = (this.y2 + ((-16 + ((int)(global::System.Math.Floor(((double)((33 * global::HaxeMath.rand.NextDouble())))))))));
                            int max3 = (this.yBound - 1);
                            if ((0 > max3))
                            {
                                throw ((global::System.Exception)(global::haxe.Exception.thrown("FAIL: min <= max")));
                            }

                            this.y2 = (((value3 < 0)) ? (0) : ((((value3 > max3)) ? (max3) : (value3))));
                            break;
                        }

                    case 2:
                        {
                            symbol = symbols[Std.random(symbols.Count)];
                        }
                        break;
                }

            }
        }


        public virtual Shape clone()
        {
            return new SymbolShape(this.xBound, this.yBound)
            {
                x1 = this.x1,
                x2 = this.x2,
                y1 = this.y1,
                y2 = this.y2,
                symbol = this.symbol,
            };
        }


        public virtual int getType()
        {
            return 7;
        }


        public virtual HaxeArray<double> getRawShapeData()
        {
            return new HaxeArray<double>(new double[] {
                (this.x1 < this.x2) ? this.x1 : this.x2, 
                (this.y1 < this.y2) ? this.y1 : this.y2, 
                (this.x1 > this.x2) ? this.x1 : this.x2, 
                (this.y1 > this.y2) ? this.y1 : this.y2,
                this.symbol.SymbolId,
            });
        }


        public virtual string getSvgShapeData()
        {
            int first = this.x1;
            int second = this.x2;
            int first1 = this.y1;
            int second1 = this.y2;
            int first2 = this.x1;
            int second2 = this.x2;
            int first3 = this.x1;
            int second3 = this.x2;
            int first4 = this.y1;
            int second4 = this.y2;
            int first5 = this.y1;
            int second5 = this.y2;
            return global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat("<rect x=\"", global::haxe.lang.Runtime.toString(((((first < second)) ? (first) : (second))))), "\" y=\""), global::haxe.lang.Runtime.toString(((((first1 < second1)) ? (first1) : (second1))))), "\" width=\""), global::haxe.lang.Runtime.toString(((((((first2 > second2)) ? (first2) : (second2))) - ((((first3 < second3)) ? (first3) : (second3))))))), "\" height=\""), global::haxe.lang.Runtime.toString(((((((first4 > second4)) ? (first4) : (second4))) - ((((first5 < second5)) ? (first5) : (second5))))))), "\" "), global::geometrize.exporter.SvgExporter.SVG_STYLE_HOOK), " />");
        }
    }
}
