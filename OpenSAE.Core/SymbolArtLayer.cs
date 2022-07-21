using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace OpenSAE.Core
{
    public class SymbolArtLayer : SymbolArtItem
    {
        public int SymbolId { get; set; }

        public Color Color { get; set; }

        public double Alpha { get; set; }

        // Symbol arts have limited precision, the coordinates used do not support floating point.
        // but because this makes manipulating them not loose precision, we want to use floating points
        // whenever we manipulate them in the app.
        // For this we use the Point struct

        /// <summary>
        /// First vertex - top left
        /// </summary>
        public Point Vertex1 { get; set; }

        /// <summary>
        /// Second vertex - bottom left
        /// </summary>
        public Point Vertex2 { get; set; }

        /// <summary>
        /// Third vertex - top right
        /// </summary>
        public Point Vertex3 { get; set; }

        /// <summary>
        /// Fouth vertex - bottom right
        /// </summary>
        public Point Vertex4 { get; set; }
    }
}
