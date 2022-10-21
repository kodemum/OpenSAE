using OpenSAE.Core;

namespace OpenSAE.CLI
{
    public static class InfoCommand
    {
        public static int ShowInfo(InfoVerb verb)
        {
            try
            {
                var sa = SymbolArt.LoadFromFile(verb.InputPath);

                if (verb.AsJson)
                {
                    Console.WriteLine(SymbolArtUtil.SerializeInfoToJson(sa));
                }
                else
                {
                    ConsoleUtil.WriteConsoleHeader();
                    ConsoleUtil.WriteNamedProperty("Filename", verb.InputPath);
                    ConsoleUtil.WriteNamedProperty("Format", sa.FileFormat);
                    ConsoleUtil.WriteNamedProperty("Symbol art name", sa.Name, ConsoleColor.Yellow);
                    ConsoleUtil.WriteNamedProperty("Size", $"{sa.Width} x {sa.Height}");
                    ConsoleUtil.WriteNamedProperty("Sound effect", sa.Sound);
                    ConsoleUtil.WriteNamedProperty("Author ID", sa.AuthorId);
                    ConsoleUtil.WriteNamedProperty("Symbol count", SymbolArtUtil.GetLayerCount(sa));
                }
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
