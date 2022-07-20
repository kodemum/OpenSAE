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
            return FlipX(points, (points.Max(x => x.X) + points.Min(x => x.X)) / 2);
        }

        public static SymbolArtPoint[] FlipY(SymbolArtPoint[] points)
        {
            return FlipY(points, (points.Max(x => x.Y) + points.Min(x => x.Y)) / 2);
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
    }
}
