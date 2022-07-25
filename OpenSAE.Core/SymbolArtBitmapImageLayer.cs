namespace OpenSAE.Core
{
    /// <summary>
    /// Layer that contains a bitmap image to help with creating symbol arts.
    /// These images cannot be embedded in the completed symbol arts.
    /// </summary>
    public class SymbolArtBitmapImageLayer : SymbolArtLayerBase
    {
        /// <summary>
        /// The bitmap image data for the layer
        /// </summary>
        public byte[]? ImageData { get; set; }
    }
}
