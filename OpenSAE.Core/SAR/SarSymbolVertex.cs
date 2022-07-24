using System.Windows;

namespace OpenSAE.Core.SAR
{
    public class SarSymbolVertex
    {
        public SarSymbolVertex(byte x, byte y)
        {
            X = x;
            Y = y;
        }

        public byte X { get; set; }

        public byte Y { get; set; }

        public Point ToPoint() 
            => new(X + SarFileConstants.GridOrigin, Y + SarFileConstants.GridOrigin);

        public static SarSymbolVertex FromPoint(Point point)
        {
            // Clamp values to maximum possible value.
            // This will change the shape of the vertex, but we'll assume it's a background
            // element and that it is okay
            double x = Math.Clamp(point.X - SarFileConstants.GridOrigin, 0, 255);
            double y = Math.Clamp(point.Y - SarFileConstants.GridOrigin, 0, 255);

            return new((byte)Math.Round(x), (byte)Math.Round(y));
        }
    }
}
