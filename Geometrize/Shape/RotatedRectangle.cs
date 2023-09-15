// Generated by Haxe 4.3.1

#pragma warning disable 109, 114, 219, 429, 168, 162
using Geometrize.Rasterizer;
using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace Geometrize.Shape
{
    public class RotatedRectangle : Rectangle
    {
        public RotatedRectangle(int xBound, int yBound)
            : base(xBound, yBound)
        {
            angle = (int)Math.Floor((double)(361 * HaxeMath.rand.NextDouble()));
        }

        public int angle;

        public override IReadOnlyList<Scanline> Rasterize()
        {
            var points = getCornerPoints();

            return Scanline.Trim(Rasterizer.Rasterizer.ScanlinesForPolygon(points), xBound, yBound);
        }


        public override void Mutate()
        {
            unchecked
            {
                int r = Std.random(3);
                switch (r)
                {
                    case 0:
                        {
                            int @value = this.x1 + -16 + ((int)global::System.Math.Floor((double)(33 * HaxeMath.rand.NextDouble())));
                            int max = this.xBound - 1;
                            if (0 > max)
                            {
                                throw new Exception("FAIL: min <= max");
                            }

                            this.x1 = (@value < 0) ? 0 : ((@value > max) ? max : @value);
                            int value1 = this.y1 + -16 + ((int)global::System.Math.Floor((double)(33 * HaxeMath.rand.NextDouble())));
                            int max1 = this.yBound - 1;
                            if (0 > max1)
                            {
                                throw new Exception("FAIL: min <= max");
                            }

                            this.y1 = (value1 < 0) ? 0 : ((value1 > max1) ? max1 : value1);
                            break;
                        }


                    case 1:
                        {
                            int value2 = this.x2 + -16 + ((int)global::System.Math.Floor((double)(33 * HaxeMath.rand.NextDouble())));
                            int max2 = this.xBound - 1;
                            if (0 > max2)
                            {
                                throw new Exception("FAIL: min <= max");
                            }

                            this.x2 = (value2 < 0) ? 0 : ((value2 > max2) ? max2 : value2);
                            int value3 = this.y2 + -16 + ((int)global::System.Math.Floor((double)(33 * HaxeMath.rand.NextDouble())));
                            int max3 = this.yBound - 1;
                            if (0 > max3)
                            {
                                throw new Exception("FAIL: min <= max");
                            }

                            this.y2 = (value3 < 0) ? 0 : ((value3 > max3) ? max3 : value3);
                            break;
                        }


                    case 2:
                        {
                            int value4 = this.angle + -4 + ((int)global::System.Math.Floor((double)(9 * HaxeMath.rand.NextDouble())));
                            this.angle = (value4 < 0) ? 0 : ((value4 > 360) ? 360 : value4);
                            break;
                        }


                }

            }
        }


        public override IShape Clone()
        {
            return new RotatedRectangle(xBound, yBound)
            {
                x1 = x1,
                y1 = y1,
                x2 = x2,
                y2 = y2,
                angle = angle
            };
        }

        public override double[] GetRawShapeData()
        {
            return new double[]
            {
                Math.Min(x1, x2),
                Math.Min(y1, y2),
                Math.Max(x1, x2),
                Math.Max(y1, y2),
                angle
            };
        }


        public Point[] getCornerPoints()
        {
            unchecked
            {
                int xm1 = Math.Min(x1, x2);
                int xm2 = Math.Max(x1, x2);
                int ym1 = Math.Min(y1, y2);
                int ym2 = Math.Max(y1, y2);
                int cx = (xm1 + xm2) / 2;
                int cy = (ym1 + ym2) / 2;
                int ox1 = xm1 - cx;
                int ox2 = xm2 - cx;
                int oy1 = ym1 - cy;
                int oy2 = ym2 - cy;
                double rads = angle * Math.PI / 180.0;
                double c = Math.Cos((double)rads);
                double s = Math.Sin((double)rads);
                int ulx = (int)((ox1 * c) - (oy1 * s) + cx);
                int uly = (int)((ox1 * s) + (oy1 * c) + cy);
                int blx = (int)((ox1 * c) - (oy2 * s) + cx);
                int bly = (int)((ox1 * s) + (oy2 * c) + cy);
                int urx = (int)((ox2 * c) - (oy1 * s) + cx);
                int ury = (int)((ox2 * s) + (oy1 * c) + cy);
                int brx = (int)((ox2 * c) - (oy2 * s) + cx);
                int bry = (int)((ox2 * s) + (oy2 * c) + cy);

                return new Point[]
                {
                    new Point(ulx, uly),
                    new Point(urx, ury),
                    new Point(brx, bry),
                    new Point(blx, bly)
                };
            }
        }

    }
}

