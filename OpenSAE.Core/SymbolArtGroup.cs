using System.Xml.Serialization;

namespace OpenSAE.Core
{
    public class SymbolArtGroup : SymbolArtItem
    {
        public List<SymbolArtItem> Children { get; set; }
            = new();
    }
}
