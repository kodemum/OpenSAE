using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenSAE.Core
{
    public static class PointExtensions
    {
        public static Point Multiply(this Point point, double val)
        {
            return new Point(point.X * val, point.Y * val);
        }

        public static Point Round(this Point point)
        {
            return new Point(Math.Round(point.X), Math.Round(point.Y));
        }

        /// <summary>
        /// For an array of 4 vertices, finds the opposite to the specified index
        /// </summary>
        /// <param name="points"></param>
        /// <param name="index">Index of vertex where the opposite should be found</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if the array does not contain 4 vertices or the specified index is not valid</exception>
        public static Point GetOppositeVertex(this Point[] points, int index)
        {
            if (points.Length != 4)
                throw new ArgumentException("Method only works on arrays with 4 vertices", nameof(points));

            if (index < 0 || index >= points.Length)
                throw new ArgumentException("Index does not exist in the array", nameof(index));

            return points[(index + 2) % 4];
        }

        /// <summary>
        /// Checks if the specified point is inside of a polygon
        /// </summary>
        public static bool IsPointInside(this Point[] polygon, Point testPoint)
        {
            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }

            return result;
        }

        /// <summary>
        /// Checks if any lines of the polygon intersects with another polygon.
        /// </summary>
        /// <param name="polygon1"></param>
        /// <param name="polygon2"></param>
        /// <returns></returns>
        public static bool IntersectsWith(this Point[] polygon1, Point[] polygon2)
        {
            for (int i = 0; i < polygon1.Length; i++)
            {
                Point point1 = polygon1[i];
                Point point2 = polygon1.NextOrWrap(i);
                
                double a1 = point2.Y - point1.Y;
                double b1 = point1.X - point2.X;
                double c1 = a1 * (point1.X) + b1 * (point1.Y);

                double minX = Math.Min(point1.X, point2.X);
                double minY = Math.Min(point1.Y, point2.Y);
                double maxX = Math.Max(point1.X, point2.X);
                double maxY = Math.Max(point1.Y, point2.Y);

                for (int j = 0; j < polygon2.Length; j++)
                {
                    Point targetPoint1 = polygon2[j];
                    Point targetPoint2 = polygon2.NextOrWrap(j);

                    double a2 = targetPoint2.Y - targetPoint1.Y;
                    double b2 = targetPoint1.X - targetPoint2.X;
                    double c2 = a2 * (targetPoint1.X) + b2 * (targetPoint1.Y);

                    double det = a1 * b2 - a2 * b1;

                    // lines intersect, check point is on both line segments
                    if (det != 0)
                    {
                        double x = (b2 * c1 - b1 * c2) / det;
                        double y = (a1 * c2 - a2 * c1) / det;

                        double minX2 = Math.Min(targetPoint1.X, targetPoint2.X);
                        double minY2 = Math.Min(targetPoint1.Y, targetPoint2.Y);
                        double maxX2 = Math.Max(targetPoint1.X, targetPoint2.X);
                        double maxY2 = Math.Max(targetPoint1.Y, targetPoint2.Y);

                        if (minX <= x && x <= maxX && minY <= y && y <= maxY
                            && minX2 <= x && x <= maxX2 && minY2 <= y && y <= maxY2)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
