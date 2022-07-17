using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenSAE.Core
{
    [XmlRoot(ElementName = "sa")]
    public class SymbolArt : SymbolArtItem, ISymbolArtGroup
    {
        [XmlAttribute("version")]
        public int Version { get; set; }

        [XmlAttribute("author")]
        public string? Author { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlAttribute("sound")]
        public SymbolArtSoundEffect Sound { get; set; }

        [XmlElement(ElementName = "g", Type = typeof(SymbolArtGroup))]
        [XmlElement(ElementName = "layer", Type = typeof(SymbolArtLayer))]
        public List<SymbolArtItem> Children { get; set; }
            = new();

        [XmlIgnore]
        public SymbolArtFileFormat FileFormat { get; set; }

        public void Save(Stream outputStream, SymbolArtFileFormat format)
        {
            switch (format)
            {
                case SymbolArtFileFormat.SAML:
                    SAML.SamlLoader.SaveToStream(outputStream, this);
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
                Version = 4,
                Author = "0",
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
                        return SAML.SamlLoader.LoadFromStream(fs);
                    }

                default:
                    throw new ArgumentException($"File extension {Path.GetExtension(filename)} not recognized as a symbol art");
            }
        }
    }
}
