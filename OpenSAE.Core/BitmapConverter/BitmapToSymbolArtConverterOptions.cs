namespace OpenSAE.Core.BitmapConverter
{
    public class BitmapToSymbolArtConverterOptions
    {
        public double SizeXOffset { get; set; } = 0.35;

        public double SizeYOffset { get; set; } = 0.3;

        public int ResizeImageHeight { get; set; } = 24;

        public int MaxColors { get; set; } = 30;

        public bool RemoveWhite { get; set; } = false;

        public bool DisableLayering { get; set; } = false;

        public double OffsetSizeYExponent { get; set; } = 0.75;

        public double OffsetSizeXExponent { get; set; } = 0.7;

        public double CenterYOffset { get; set; } = 0.25;

        public double CenterXOffset { get; set; } = -0.20;

        public bool SmoothResizing { get; set; } = false;
    }
}
