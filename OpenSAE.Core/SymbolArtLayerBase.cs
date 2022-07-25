using System.Windows;

namespace OpenSAE.Core
{
    public abstract class SymbolArtLayerBase : SymbolArtItem
    {
        /// <summary>
        /// Opacity of the layer specified as a number between 0 (fully transparent) and 1 (fully opaque)
        /// </summary>
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
        /// Third vertex - bottom right
        /// </summary>
        public Point Vertex3 { get; set; }

        /// <summary>
        /// Fouth vertex - top right
        /// </summary>
        public Point Vertex4 { get; set; }
    }
}
