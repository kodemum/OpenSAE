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
            var s = Math.Sin(Math.PI * angle / 180);
            var c = Math.Cos(Math.PI * angle / 180);
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
    }
}
