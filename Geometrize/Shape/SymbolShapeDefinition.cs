using System;

namespace Geometrize.Shape
{
    public class SymbolShapeDefinition
    {
        public int SymbolId { get; set; }

        public byte[,] SymbolScanlines { get; set; }

        public bool HorizontallySymmetric { get; set; }

        public bool VerticallySymmetric { get; set; }

        public byte[] TakeAlphaBytes(int y, int startX, int count)
        {
            byte[] result = new byte[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = SymbolScanlines[y, startX + i];
            }

            return result;
        }
    }
}
