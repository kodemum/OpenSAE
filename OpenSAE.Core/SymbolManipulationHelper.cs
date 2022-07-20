using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Core
{
    /// <summary>
    /// Contains helpeng methods for manipulating symbol points
    /// </summary>
    public static class SymbolManipulationHelper
    {
        public static SymbolArtPoint[] FlipX(SymbolArtPoint[] points)
        {
            return FlipX(points, points.GetCenterX());
        }

        public static SymbolArtPoint[] FlipY(SymbolArtPoint[] points)
        {
            return FlipY(points, points.GetCenterY());
        }

        public static SymbolArtPoint[] FlipX(SymbolArtPoint[] points, double flipOrigin)
        {
            SymbolArtPoint[] flipped = new SymbolArtPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                flipped[i] = new SymbolArtPoint(flipOrigin - (points[i].X - flipOrigin), points[i].Y);
            }
            return flipped;
        }

        public static SymbolArtPoint[] FlipY(SymbolArtPoint[] points, double flipOrigin)
        {
            SymbolArtPoint[] flipped = new SymbolArtPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                flipped[i] = new SymbolArtPoint(points[i].X, flipOrigin - (points[i].Y - flipOrigin));
            }
            return flipped;
        }

        public static SymbolArtPoint[] Rotate(SymbolArtPoint[] points, double angle)
        {
            return Rotate(points, points.GetCenter(), angle);
        }

        public static SymbolArtPoint[] Rotate(SymbolArtPoint[] points, SymbolArtPoint origin, double angle)
        {
            var s = Math.Sin(Math.PI * angle / 180);
            var c = Math.Cos(Math.PI * angle / 180);

            SymbolArtPoint[] result = new SymbolArtPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var translated = new SymbolArtPoint(points[i].X - origin.X, points[i].Y - origin.Y);

                var n = new SymbolArtPoint(translated.X * c - translated.Y * s, translated.X * s + translated.Y * c);

                result[i] = n + origin;
            }
            return result;
        }
    }
}
