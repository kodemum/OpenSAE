using System.Windows.Media;

namespace OpenSAE.Core.SAR
{
    public static class SarFileConstants
    {
        public const uint Layer_IsHidden = 0x80000000;
        public const uint Mask_SymbolId = 0x7FE00000;
        public const uint Mask_Alpha = 0x1C0000;
        public const uint Mask_ColorR = 0x3F;
        public const uint Mask_ColorG = 0xFC0;
        public const uint Mask_ColorB = 0x3F000;

        public const int GridOrigin = -128;
    }
}
