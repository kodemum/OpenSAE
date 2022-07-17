using System;
using System.Collections.Generic;
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
    }
}
