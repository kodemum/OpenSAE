using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Core
{
    public class SymbolArt : SymbolArtGroup, ISymbolArtGroup
    {
        public uint AuthorId { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public SymbolArtSoundEffect Sound { get; set; }

        public SymbolArtFileFormat FileFormat { get; set; }

        public void Save(Stream outputStream, SymbolArtFileFormat format)
        {
            ISymbolArtFileFormat fileFormat = format switch
            {
                SymbolArtFileFormat.SAML => new SAML.SamlFileFormat(),
                SymbolArtFileFormat.SAR => new SAR.SarFileFormat(),
                _ => throw new ArgumentException("Unknown file format")
            };

            fileFormat.SaveToStream(this, outputStream);
        }

        public static SymbolArt CreateBlank(string name)
        {
            return new SymbolArt()
            {
                Name = name,
                AuthorId = 0,
                Width = 192,
                Height = 96,
                Sound = SymbolArtSoundEffect.None,
                Visible = true
            };
        }

        public static SymbolArt LoadFromFile(string filename)
        {
            ISymbolArtFileFormat format = Path.GetExtension(filename).ToLowerInvariant() switch
            {
                ".saml" => new SAML.SamlFileFormat(),
                ".sar" => new SAR.SarFileFormat(),
                _ => throw new ArgumentException($"File extension {Path.GetExtension(filename)} not recognized as a symbol art"),
            };

            using var fs = File.OpenRead(filename);

            return format.LoadFromStream(fs);
        }
    }
}
