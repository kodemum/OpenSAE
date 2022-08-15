using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OpenSAE.Core
{
    /// <summary>
    /// Maps colors to color names
    /// </summary>
    public class ColorNameMapper
    {
        private static readonly Dictionary<int, string> colorMap = new();

        private static readonly HashSet<KnownColor> ignoredColors = new()
        {
            KnownColor.ActiveBorder,
            KnownColor.ActiveCaption,
            KnownColor.ActiveCaptionText,
            KnownColor.AppWorkspace,
            KnownColor.Control,
            KnownColor.ControlDark,
            KnownColor.ControlDarkDark,
            KnownColor.ControlLight,
            KnownColor.ControlLightLight,
            KnownColor.ControlText,
            KnownColor.Desktop,
            KnownColor.GrayText,
            KnownColor.Highlight,
            KnownColor.HighlightText,
            KnownColor.HotTrack,
            KnownColor.InactiveBorder,
            KnownColor.InactiveCaption,
            KnownColor.InactiveCaptionText,
            KnownColor.Info,
            KnownColor.InfoText,
            KnownColor.Menu,
            KnownColor.MenuText,
            KnownColor.ScrollBar,
            KnownColor.Window,
            KnownColor.WindowText,
            KnownColor.WindowFrame,
            KnownColor.Transparent,
            KnownColor.ButtonFace,
            KnownColor.ButtonHighlight,
            KnownColor.ButtonShadow,
            KnownColor.GradientActiveCaption,
            KnownColor.GradientInactiveCaption,
            KnownColor.MenuBar,
            KnownColor.MenuHighlight,
            KnownColor.Snow,
        };

        static ColorNameMapper()
        {
            foreach (KnownColor kc in Enum.GetValues(typeof(KnownColor)))
            {
                if (!ignoredColors.Contains(kc))
                {
                    var c = System.Drawing.Color.FromKnownColor(kc);

                    colorMap.TryAdd(c.ToArgb() & 0x00FFFFFF, c.Name);
                }
            }
        }

        public static string? GetName(System.Windows.Media.Color color)
        {
            int myRgb = ToArgb(color);
            if (colorMap.ContainsKey(myRgb))
            {
                return colorMap[myRgb];
            }

            return null;
        }

        public static string? GetNearestName(System.Windows.Media.Color color)
        {
            string? name = GetName(color);
            if (name != null)
            {
                return name;
            }

            int myRgb = ToArgb(color);
            int? closestColor = colorMap.Keys.Select(colorKey => new ColorDistance(colorKey, myRgb)).MinBy(d => d.Distance)?.ColorKey;

            if (closestColor == null)
                return null;
            else
                return colorMap[closestColor.Value];
        }

        private static int ToArgb(System.Windows.Media.Color color)
        {
            return (color.R << 16) + (color.G << 8) + color.B;
        }

        // Just a simple utility class to store our
        // color values and the distance from the color of interest
        private class ColorDistance
        {
            public int ColorKey { get; }

            public int Distance { get; }

            public ColorDistance(int colorKeyRgb, int rgb2)
            {
                //store for use at end of query
                ColorKey = colorKeyRgb;

                //we just pull the individual color components out
                byte r1 = (byte)((colorKeyRgb >> 16) & 0xff);
                byte g1 = (byte)((colorKeyRgb >> 8) & 0xff);
                byte b1 = (byte)((colorKeyRgb) & 0xff);

                byte r2 = (byte)((rgb2 >> 16) & 0xff);
                byte g2 = (byte)((rgb2 >> 8) & 0xff);
                byte b2 = (byte)((rgb2) & 0xff);

                //provide a simple distance measure between colors
                Distance = Math.Abs(r1 - r2) + Math.Abs(g1 - g2) + Math.Abs(b1 - b2);
            }
        }
    }
}
