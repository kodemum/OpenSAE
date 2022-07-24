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
    }
}
