using System.Xml.Serialization;

namespace OpenSAE.Core
{
    public abstract class SymbolArtItem
    {
        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlAttribute("visible")]
        public bool Visible { get; set; }
    }
}
