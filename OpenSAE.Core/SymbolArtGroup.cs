using System.Xml.Serialization;

namespace OpenSAE.Core
{
    public class SymbolArtGroup : SymbolArtItem, ISymbolArtGroup
    {
        public List<SymbolArtItem> Children { get; set; }
            = new();
    }
}
