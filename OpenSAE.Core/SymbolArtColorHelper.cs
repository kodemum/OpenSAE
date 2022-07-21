using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OpenSAE.Core
{
    public static class SymbolArtColorHelper
    {
        private static readonly byte[] ColorMap = new byte[]
        {
            0, 1, 3, 4, 5, 7, 8, 10, 12, 14, 18, 18, 20, 22, 24, 27,
            29, 32, 35, 38, 41, 44, 47, 50, 53, 56, 60, 63, 67, 71, 75,
            79, 83, 87, 91, 95, 100, 104, 109, 114, 118, 123, 128, 133,
            138, 144, 149, 155, 160, 166, 171, 177, 183, 189, 195, 202,
            208, 214, 221, 227, 234, 241, 248, 255
        };

        private static readonly byte[] ReverseColorMap;

        static SymbolArtColorHelper()
        {
            ReverseColorMap = new byte[256];

            int i = 0;
            foreach (byte level in ColorMap)
            {
                for (int j = i; j <= level; j++)
                {
                    ReverseColorMap[j] = level;
                }
            }
        }

        public static Color ApplyCurve(Color input)
        {
            return Color.FromRgb(ApplyCurve(input.R), ApplyCurve(input.G), ApplyCurve(input.B));
        }

        public static byte ApplyCurve(byte level)
        {
            return ColorMap[level / 4];
        }

        public static Color ConvertToColor(byte r, byte g, byte b)
            => Color.FromRgb(SixToEightBit(r), SixToEightBit(g), SixToEightBit(b));

        public static byte SixToEightBit(byte input)
        {
            return (byte)(input * 4);
        }
    }
}
