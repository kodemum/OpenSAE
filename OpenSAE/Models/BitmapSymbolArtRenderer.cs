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
using System.Windows.Threading;

namespace OpenSAE.Models
{
    public class BitmapSymbolArtRenderer
    {
        private readonly Grid _grid;
        private readonly Views.SymbolArtRenderer _renderer;

        public BitmapSymbolArtRenderer()
        {
            _grid = new()
            {
                Width = 192,
                Height = 96
            };

            _renderer = new(true, true)
            {
                ApplyToneCurve = true,
                ShowGuides = false,
                ShowBoundingBox = false,
                DisableGridPositioning = true,
                SymbolUnitWidth = 192,
            };

            _grid.Children.Add(_renderer);
        }

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
            SolidColorBrush? backgroundBrush = (encoder is JpegBitmapEncoder || encoder is BmpBitmapEncoder) ? new SolidColorBrush(Colors.White) : null;

            var renderTarget = new BitmapSymbolArtRenderer().RenderToBitmapTarget(sa, width, height, backgroundBrush);

            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            encoder.Save(outputStream);
        }

        public RenderTargetBitmap RenderToBitmapTarget(SymbolArtModel sa, int width, int height, Brush? backgroundBrush = null)
        {
            _grid.Width = width * 2;
            _grid.Height = height * 2;
            _renderer.SymbolUnitWidth = sa.Width;
            _renderer.SymbolArt = sa;

            _grid.Background = backgroundBrush;

            _grid.Measure(new Size(_grid.Width, _grid.Height));
            _grid.Arrange(new Rect(0, 0, _grid.Width, _grid.Height));

            RenderTargetBitmap renderTarget = new((int)_grid.Width, (int)_grid.Height, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(_grid);

            return renderTarget;
        }
    }
}
