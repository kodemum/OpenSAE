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
    }
}
