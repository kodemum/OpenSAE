using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenSAE.Core
{
    /// <summary>
    /// Contains helpeng methods for manipulating symbol points
    /// </summary>
    public static class SymbolManipulationHelper
    {
        public static Point[] FlipX(Point[] points)
        {
            return FlipX(points, points.GetCenterX());
        }

        public static Point[] FlipY(Point[] points)
        {
            return FlipY(points, points.GetCenterY());
        }

        public static Point[] FlipX(Point[] points, double flipOrigin)
        {
            Point[] flipped = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                flipped[i] = new Point(flipOrigin - (points[i].X - flipOrigin), points[i].Y);
            }
            return flipped;
        }

        public static Point[] FlipY(Point[] points, double flipOrigin)
        {
            Point[] flipped = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                flipped[i] = new Point(points[i].X, flipOrigin - (points[i].Y - flipOrigin));
            }
            return flipped;
        }

        public static Point[] Rotate(Point[] points, double angle)
        {
            return Rotate(points, points.GetCenter(), angle);
        }

        public static Point[] Rotate(Point[] points, Point origin, double angle)
        {
            var s = Math.Sin(angle);
            var c = Math.Cos(angle);
            var originVector = new Vector(origin.X, origin.Y);

            Point[] result = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var translated = new Point(points[i].X - origin.X, points[i].Y - origin.Y);

                var n = new Point(translated.X * c - translated.Y * s, translated.X * s + translated.Y * c);

                result[i] = n + originVector;
            }
            return result;
        }

        public static Point[] CenterArbitrarySizeInArea(double areaWidth, double areaHeight, double width, double height)
        {
            double offsetX = -(areaWidth / 2), offsetY = -(areaHeight / 2);
            
            var aspect = width / height;
            if (aspect < 2)
            {
                // image is less wide than a typical symbol art - restrict height
                height = areaHeight;
                width = height * aspect;
                offsetX += (areaWidth - width) / 2;
            }
            else
            {
                // image equal to or wider than a typical symbol art - restrict width
                width = areaWidth;
                height = width / aspect;
                offsetY += (areaHeight - height) / 2;
            }

            return new[]
            {
                new Point(offsetX, offsetY),
                new Point(offsetX, offsetY + height),
                new Point(offsetX + width, offsetY + height),
                new Point(offsetX + width, offsetY)
            };
        }

        /// <summary>
        /// Returns a new vector with the X and Y axes averaged relative to the specified aspect ratio.
        /// </summary>
        /// <param name="vector">Vector to average</param>
        /// <param name="aspectRatio">Aspect ratio of the shape relative to the vector</param>
        /// <returns></returns>
        public static Vector AverageForAspectRatio(Vector vector, double aspectRatio)
        {
            double avgVector = (vector.X + vector.Y * aspectRatio) / 2;
            return new Vector(avgVector, avgVector / aspectRatio);
        }
    }
}
