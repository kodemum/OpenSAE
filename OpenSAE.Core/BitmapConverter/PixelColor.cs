using SixLabors.ImageSharp.PixelFormats;

namespace OpenSAE.Core.BitmapConverter
{
    internal class PixelColor
    {
        public Rgba32 Color { get; set; }

        public List<(int X, int Y)> Pixels { get; } = new();

        public PixelColor(Rgba32 color, int x, int y)
        {
            Color = color;
            Pixels.Add((x, y));
        }
    }
}
