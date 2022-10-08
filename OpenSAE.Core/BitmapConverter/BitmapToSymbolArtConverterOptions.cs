namespace OpenSAE.Core.BitmapConverter
{
    public class BitmapToSymbolArtConverterOptions
    {
        public double SizeOffset { get; set; } = 0.65;

        public int ResizeImageHeight { get; set; } = 24;

        public int MaxColors { get; set; } = 30;

        public bool RemoveWhite { get; set; } = true;
    }
}
