using System.Xml.Serialization;

namespace OpenSAE.Core
{
    public class SymbolArtGroup : SymbolArtItem, ISymbolArtGroup
    {
        [XmlElement(ElementName = "g", Type = typeof(SymbolArtGroup))]
        [XmlElement(ElementName = "layer", Type = typeof(SymbolArtLayer))]
        public List<SymbolArtItem> Children { get; set; }
            = new();
    }
}
