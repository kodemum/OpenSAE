// Generated by Haxe 4.3.1

#pragma warning disable 109, 114, 219, 429, 168, 162
using Geometrize.Rasterizer;
using System.Collections.Generic;

namespace Geometrize.Shape
{
    public class Line : IShape
    {
        public Line(int xBound, int yBound)
        {
            x1 = Std.random(xBound);
            y1 = Std.random(yBound);
            int @value = ((x1 + Std.random(32)) + 1);
            if ((0 > xBound))
            {
                throw new System.Exception("FAIL: min <= max");
            }

            x2 = (((@value < 0)) ? (0) : ((((@value > xBound)) ? (xBound) : (@value))));
            int value1 = ((y1 + Std.random(32)) + 1);
            if ((0 > yBound))
            {
                throw new System.Exception("FAIL: min <= max");
            }

            y2 = (((value1 < 0)) ? (0) : ((((value1 > yBound)) ? (yBound) : (value1))));
            this.xBound = xBound;
            this.yBound = yBound;
        }

        public int x1;

        public int y1;

        public int x2;

        public int y2;

        public int xBound;

        public int yBound;

        public virtual IReadOnlyList<Scanline> Rasterize()
        {
            var lines = new List<Scanline>();
            var points = Rasterizer.Rasterizer.Bresenham(this.x1, this.y1, this.x2, this.y2);

            foreach (var point in points)
            {
                lines.Add(new Scanline(point.Y, point.X, point.X));
            }

            return Scanline.Trim(lines, this.xBound, this.yBound);
        }


        public virtual void Mutate()
        {
            unchecked
            {
                int r = Std.random(4);
                switch (r)
                {
                    case 0:
                        {
                            int @value = (this.x1 + ((-16 + ((int)(System.Math.Floor(((double)((33 * HaxeMath.rand.NextDouble())))))))));
                            int max = (this.xBound - 1);
                            if ((0 > max))
                            {
                                throw new System.Exception("FAIL: min <= max");
                            }

                            this.x1 = (((@value < 0)) ? (0) : ((((@value > max)) ? (max) : (@value))));
                            int value1 = (this.y1 + ((-16 + ((int)(System.Math.Floor(((double)((33 * HaxeMath.rand.NextDouble())))))))));
                            int max1 = (this.yBound - 1);
                            if ((0 > max1))
                            {
                                throw new System.Exception("FAIL: min <= max");
                            }

                            this.y1 = (((value1 < 0)) ? (0) : ((((value1 > max1)) ? (max1) : (value1))));
                            break;
                        }


                    case 1:
                        {
                            int value2 = (this.x2 + ((-16 + ((int)(System.Math.Floor(((double)((33 * HaxeMath.rand.NextDouble())))))))));
                            int max2 = (this.xBound - 1);
                            if ((0 > max2))
                            {
                                throw new System.Exception("FAIL: min <= max");
                            }

                            this.x2 = (((value2 < 0)) ? (0) : ((((value2 > max2)) ? (max2) : (value2))));
                            int value3 = (this.y2 + ((-16 + ((int)(System.Math.Floor(((double)((33 * HaxeMath.rand.NextDouble())))))))));
                            int max3 = (this.yBound - 1);
                            if ((0 > max3))
                            {
                                throw new System.Exception("FAIL: min <= max");
                            }

                            this.y2 = (((value3 < 0)) ? (0) : ((((value3 > max3)) ? (max3) : (value3))));
                            break;
                        }


                }

            }
        }


        public virtual IShape Clone()
        {
            return new Line(xBound, yBound)
            {
                x1 = x1,
                y1 = y1,
                x2 = x2,
                y2 = y2
            };
        }

        public virtual double[] GetRawShapeData()
        {
            return new double[]
            {
                x1, y1, x2, y2
            };
        }
    }
}


