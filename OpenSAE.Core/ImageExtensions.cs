using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenSAE.Core
{
    public static class ImageExtensions
    {
        public static System.Windows.Media.Color ToWindowsMediaColor(this Rgba32 color)
            => System.Windows.Media.Color.FromRgb(color.R, color.G, color.B);

        public static Rgba32 FindMostCommonColor(this Image<Rgba32> image)
        {
            Dictionary<Rgba32, int> colors = new();

            image.ProcessPixelRows(accessor =>
            {
                for (int rowi = 0; rowi < accessor.Height; rowi++)
                {
                    var row = accessor.GetRowSpan(rowi);

                    for (int coli = 0; coli < row.Length; coli++)
                    {
                        var color = row[coli];

                        if (colors.TryGetValue(color, out int count))
                        {
                            colors[color] = count + 1;
                        }
                        else
                        {
                            colors[color] = 1;
                        }
                    }
                }
            });

            return colors.OrderByDescending(x => x.Value).First().Key;
        }
    }
}
