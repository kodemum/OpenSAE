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
    }
}
