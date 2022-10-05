using CommandLine;
using CommandLine.Text;
using OpenSAE.Models;
using System.Windows.Media.Imaging;

namespace OpenSAE.CLI
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("OpenSAE command-line interface");

            var result = Parser.Default.ParseArguments<RenderVerb>(Environment.GetCommandLineArgs().Skip(1));

            result.WithParsed<RenderVerb>(RunRemap);

            void RunRemap(RenderVerb verb)
            {
                try
                {
                    Core.Symbol.DisablePreloading = true;

                    SymbolArtModel sa = new(new DummyUndoModel(), verb.InputPath);
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

                    using FileStream fs = File.Open(verb.OutputPath, FileMode.Create);

                    BitmapSymbolArtRenderer.RenderToStream(sa, encoder, width, height, fs);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.ToString());
                    Console.ResetColor();

                    Environment.Exit(1);
                }
            }

            result.WithNotParsed(x =>
            {
                var helpText = HelpText.AutoBuild(result, h => HelpText.DefaultParsingErrorsHandler(result, h), e => e, verbsIndex: true);

                Console.WriteLine(helpText);

                Environment.Exit(-1);
            });
        }
    }
}