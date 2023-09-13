using SixLabors.ImageSharp.PixelFormats;

namespace OpenSAE.Core.BitmapConverter
{
    internal static class GeometrizeUtil
    {
        public static int ColorToInt(System.Windows.Media.Color color)
            => (color.R << 24) + (color.G << 16) + (color.B << 8) + color.A;

        public static System.Windows.Media.Color IntToColor(int color)
            => System.Windows.Media.Color.FromArgb((byte)(color & 255), (byte)((color >> 24) & 255), (byte)((color >> 16) & 255), (byte)((color >> 8) & 255));
    }
}
