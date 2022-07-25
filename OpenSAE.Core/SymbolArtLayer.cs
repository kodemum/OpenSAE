using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace OpenSAE.Core
{
    /// <summary>
    /// Standard layer that contains a symbol
    /// </summary>
    public class SymbolArtLayer : SymbolArtLayerBase
    {
        public int SymbolId { get; set; }

        public Color Color { get; set; }

        /// <summary>
        /// Index of the layer in the order loaded. This is only used for displaying
        /// a title of layers with no name
        /// </summary>
        public int Index { get; set; }
    }
}
