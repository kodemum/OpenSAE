using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenSAE.Models
{
    internal class MatrixEx
    {
        public double m11;
        public double m12;
        public double m13;
        public double m21;
        public double m22;
        public double m23;
        public double m31;
        public double m32;
        public double m33;

        public MatrixEx(
            double m11, double m12, double m13, 
            double m21, double m22, double m23, 
            double m31, double m32, double m33)
        {
            this.m11 = m11;
            this.m12 = m12;
            this.m13 = m13;
            this.m21 = m21;
            this.m22 = m22;
            this.m23 = m23;
            this.m31 = m31;
            this.m32 = m32;
            this.m33 = m33;
        }

        private void Transform(ref double x, ref double y)
        {
            int z = 1;

            double r0 = m11 * x + m12 * y + m13 * z;
            double r1 = m21 * x + m22 * y + m23 * z;
            double r2 = m31 * x + m32 * y + m33 * z;

            x = r0 / r2;
            y = r1 / r2;
        }

        public Point Transform(Point point)
        {
            double x = point.X, y = point.Y;

            Transform(ref x, ref y);

            return new Point(x, y);
        }

        public Vector Transform(Vector point)
        {
            double x = point.X, y = point.Y;

            Transform(ref x, ref y);

            return new Vector(x, y);
        }

        public MatrixEx GetAdjugate()
        {
            return new MatrixEx(
                m22 * m33 - m23 * m32,
                m13 * m32 - m12 * m33,
                m12 * m23 - m13 * m22,
                m23 * m31 - m21 * m33,
                m11 * m33 - m13 * m31,
                m13 * m21 - m11 * m23,
                m21 * m32 - m22 * m31,
                m12 * m31 - m11 * m32,
                m11 * m22 - m12 * m21
                );
        }

        public MatrixEx Multiply(MatrixEx target)
        {
            double[] output = new double[9];
            var a = ToArray();
            var b = target.ToArray();

            for (var i = 0; i != 3; ++i)
            {
                for (var j = 0; j != 3; ++j)
                {
                    double cij = 0;
                    for (var k = 0; k != 3; ++k)
                    {
                        cij += a[3 * i + k] * b[3 * k + j];
                    }
                    output[3 * i + j] = cij;
                }
            }

            return FromArray(output);
        }

        public (double x, double y, double z) MultiplyVector(double x, double y, double z)
        {
            return (
                m11*x + m12*y + m13*z,
                m21*x + m22*y + m23*z,
                m31*x + m32*y + m33*z
                );
        }

        public double[] ToArray() => new[]
        {
            m11, m12, m13,
            m21, m22, m23,
            m31, m32, m33
        };

        public static MatrixEx FromArray(double[] array) => new(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7], array[8]);

        public static MatrixEx operator *(MatrixEx m1, MatrixEx m2) => m1.Multiply(m2);

        public static MatrixEx FromVertices(Point point1, Point point2, Point point3, Point point4)
        {
            var v = point4.X / (point1.X * point2.X * point3.X);
            var u = point4.Y / (point1.Y * point2.Y * point3.Y);
            var t = 1;

            return new(
                v * point1.X,
                u * point2.X,
                t * point3.X,
                v * point1.Y,
                u * point2.Y,
                t * point3.Y,
                v,
                u,
                t
                );
        }

        public static MatrixEx General2DProjection(Point[] source, Point[] dest)
        {
            var s = BasisToPoints(source);
            var d = BasisToPoints(dest);

            return d * s.GetAdjugate();
        }

        public static MatrixEx BasisToPoints(Point[] source)
        {
            var m = new MatrixEx(
                source[0].X, source[3].X, source[1].X,
                source[0].Y, source[3].Y, source[1].Y,
                1, 1, 1);

            var (x, y, z) = m.GetAdjugate().MultiplyVector(source[2].X, source[2].Y, 1);

            return m * new MatrixEx(
                -x, 0, 0,
                0, y, 0,
                0, 0, z
                );
        }
    }
}
