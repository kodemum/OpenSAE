using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenSAE.Core.SAML
{
    [XmlRoot(ElementName = "sa")]
    public class SamlSymbolFile : SamlItem
    {
        [XmlAttribute("version")]
        public int Version { get; set; }

        [XmlAttribute("author")]
        public uint AuthorId { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlAttribute("sound")]
        public SymbolArtSoundEffect Sound { get; set; }

        [XmlElement(ElementName = "g", Type = typeof(SamlGroup))]
        [XmlElement(ElementName = "layer", Type = typeof(SamlLayer))]
        [XmlElement(ElementName = "bitmapLayer", Type = typeof(SamlBitmapImageLayer))]
        public List<SamlItem> Children { get; set; }
            = new();
    }
}
