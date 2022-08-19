using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OpenSAE.Helpers
{
    /// <summary>
    /// Implements getting the color of a pixel from a point on the screen
    /// </summary>
    internal static class ScreenColorPicker
    {
        private static Bitmap screenPixel = new(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        public static System.Windows.Media.Color GetColorAt(System.Windows.Point location)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = NativeMethods.BitBlt(hDC, 0, 0, 1, 1, hSrcDC, (int)location.X, (int)location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            var color = screenPixel.GetPixel(0, 0);

            return System.Windows.Media.Color.FromArgb(255, color.R, color.G, color.B);
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            static extern bool GetCursorPos(ref Point lpPoint);

            [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
            public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        }
    }
}
