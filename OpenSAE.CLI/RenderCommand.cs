using OpenSAE.Models;
using OpenSAE.Core;
using System.Windows.Media.Imaging;

namespace OpenSAE.CLI
{
    public static class RenderCommand
    {
        public static int Render(RenderVerb verb)
        {
            ConsoleUtil.WriteConsoleHeader();

            try
            {
                Symbol.DisablePreloading = true;

                SymbolArt sa = SymbolArt.LoadFromFile(verb.InputPath);
                SymbolArtModel sam = new(new DummyUndoModel(), sa);
                BitmapSymbolArtRenderer renderer = new();

                BitmapEncoder encoder = Path.GetExtension(verb.OutputPath).ToLowerInvariant() switch
                {
                    ".png" => new PngBitmapEncoder(),
                    ".jpg" => new JpegBitmapEncoder(),
                    ".jpeg" => new JpegBitmapEncoder(),
                    ".gif" => new GifBitmapEncoder(),
                    ".bmp" => new BmpBitmapEncoder(),
                    ".tif" => new TiffBitmapEncoder(),
                    _ => throw new InvalidOperationException($"Unknown bitmap file format {Path.GetExtension(verb.OutputPath)}")
                };

                int width = (int)((double)sa.Width * verb.RenderScale / 100);
                int height = (int)((double)sa.Height * verb.RenderScale / 100);

                var directoryName = Path.GetDirectoryName(verb.OutputPath);
                if (!string.IsNullOrEmpty(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                if (!string.IsNullOrEmpty(verb.InfoJsonPath))
                {
                    File.WriteAllText(verb.InfoJsonPath, SymbolArtUtil.SerializeInfoToJson(sa));
                }

                using FileStream fs = File.Open(verb.OutputPath, FileMode.Create);

                BitmapSymbolArtRenderer.RenderToStream(sam, encoder, width, height, fs);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();

                return 1;
            }

            return 0;
        }
    }
}
