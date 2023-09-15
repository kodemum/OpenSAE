using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Core
{
    public static class ColorExtensions
    {
        public static Rgba32 ToRgba32(this System.Windows.Media.Color color)
            => new(color.R, color.G, color.B, color.A);

        public static System.Windows.Media.Color ToMediaColor(this Rgba32 color)
            => System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}
