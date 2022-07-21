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
            => new((byte)(point.X - SarFileConstants.GridOrigin), (byte)(point.Y - SarFileConstants.GridOrigin));
    }
}
