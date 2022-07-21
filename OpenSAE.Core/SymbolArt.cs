using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Core
{
    public class SymbolArt : SymbolArtItem, ISymbolArtGroup
    {
        public uint AuthorId { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public SymbolArtSoundEffect Sound { get; set; }

        public List<SymbolArtItem> Children { get; set; }
            = new();

        public SymbolArtFileFormat FileFormat { get; set; }

        public void Save(Stream outputStream, SymbolArtFileFormat format)
        {
            switch (format)
            {
                case SymbolArtFileFormat.SAML:
                    new SAML.SamlFileFormat().SaveToStream(this, outputStream);
                    break;

                default:
                    throw new ArgumentException("Unknown file format");
            }
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
            switch (Path.GetExtension(filename).ToLowerInvariant())
            {
                case ".saml":
                    using (var fs = File.OpenRead(filename))
                    {
                        return new SAML.SamlFileFormat().LoadFromStream(fs);
                    }

                default:
                    throw new ArgumentException($"File extension {Path.GetExtension(filename)} not recognized as a symbol art");
            }
        }
    }
}
