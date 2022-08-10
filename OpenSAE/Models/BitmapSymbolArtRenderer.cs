using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenSAE.Models
{
    public static class BitmapSymbolArtRenderer
    {
        /// <summary>
        /// Renders the specified symbol art model as a bitmap by creating an instance of the specified <typeparamref name="TEncoder"/> and writes it
        /// to the specified stream.
        /// </summary>
        /// <typeparam name="TEncoder">Type of encoder to use for creating the bitmap</typeparam>
        /// <param name="sa">Symbol art model to render</param>
        /// <param name="width">Target size of the bitmap in pixels</param>
        /// <param name="height">Target height of the bitmap in pixels</param>
        /// <param name="outputStream">Stream to write bitmap to</param>
        public static void RenderToStream<TEncoder>(SymbolArtModel sa, int width, int height, Stream outputStream)
            where TEncoder : BitmapEncoder, new()
        {
            RenderToStream(sa, new TEncoder(), width, height, outputStream);
        }

        /// <summary>
        /// Renders the specified symbol art model as a bitmap using the specified <see cref="BitmapEncoder"/> and writes it
        /// to the specified stream.
        /// </summary>
        /// <param name="sa">Symbol art model to render</param>
        /// <param name="encoder">Encoder to use for creating the bitmap</param>
        /// <param name="width">Target size of the bitmap in pixels</param>
        /// <param name="height">Target height of the bitmap in pixels</param>
        /// <param name="outputStream">Stream to write bitmap to</param>
        public static void RenderToStream(SymbolArtModel sa, BitmapEncoder encoder, int width, int height, Stream outputStream)
        {
            Grid grid = new()
            { 
                Width = width, 
                Height = height 
            };

            Views.SymbolArtRenderer renderer = new(true, true)
            {
                SymbolArt = sa,
                ApplyToneCurve = true,
                ShowGuides = false,
                SymbolUnitWidth = sa.Width,
            };

            grid.Children.Add(renderer);

            if (encoder is JpegBitmapEncoder || encoder is BmpBitmapEncoder)
            {
                grid.Background = new SolidColorBrush(Colors.White);
            }

            grid.Measure(new Size(grid.Width, grid.Height));
            grid.Arrange(new Rect(0, 0, grid.Width, grid.Height));

            RenderTargetBitmap renderTarget = new((int)grid.Width, (int)grid.Height, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(grid);

            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            encoder.Save(outputStream);
        }
    }
}
