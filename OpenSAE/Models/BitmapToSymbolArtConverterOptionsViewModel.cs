using CommunityToolkit.Mvvm.ComponentModel;
using OpenSAE.Core.BitmapConverter;

namespace OpenSAE.Models
{
    internal class BitmapToSymbolArtConverterOptionsViewModel : ObservableObject
    {
        private int _resizeImageHeight;
        private int _maxColors;
        private bool _removeWhite;
        private double _centerYOffset;
        private double _centerXOffset;
        private double _offsetSizeYExponent;
        private double _offsetSizeXExponent;
        private double _symbolSizeOffsetX;
        private double _symbolSizeOffsetY;
        private bool _disableLayering;
        private bool _smoothResizing;

        public BitmapToSymbolArtConverterOptionsViewModel()
        {
            var defaultOptions = new BitmapToSymbolArtConverterOptions();
            _symbolSizeOffsetX = defaultOptions.SizeXOffset;
            _symbolSizeOffsetY = defaultOptions.SizeYOffset;
            _resizeImageHeight = defaultOptions.ResizeImageHeight;
            _maxColors = defaultOptions.MaxColors;
            _removeWhite = defaultOptions.RemoveWhite;
            _centerYOffset = defaultOptions.CenterYOffset;
            _centerXOffset = defaultOptions.CenterXOffset;
            _offsetSizeYExponent = defaultOptions.OffsetSizeYExponent;
            _offsetSizeXExponent = defaultOptions.OffsetSizeXExponent;
            _disableLayering = defaultOptions.DisableLayering;
            _smoothResizing = defaultOptions.SmoothResizing;
        }

        public double SymbolSizeOffsetX
        {
            get => _symbolSizeOffsetX;
            set => SetProperty(ref _symbolSizeOffsetX, value);
        }

        public double SymbolSizeOffsetY
        {
            get => _symbolSizeOffsetY;
            set => SetProperty(ref _symbolSizeOffsetY, value);
        }

        public double CenterYOffset
        {
            get => _centerYOffset;
            set => SetProperty(ref _centerYOffset, value);
        }

        public double CenterXOffset
        {
            get => _centerXOffset;
            set => SetProperty(ref _centerXOffset, value);
        }

        public double OffsetSizeYExponent
        {
            get => _offsetSizeYExponent;
            set => SetProperty(ref _offsetSizeYExponent, value);
        }

        public double OffsetSizeXExponent
        {
            get => _offsetSizeXExponent;
            set => SetProperty(ref _offsetSizeXExponent, value);
        }

        public int ResizeImageHeight
        {
            get => _resizeImageHeight;
            set => SetProperty(ref _resizeImageHeight, value);
        }

        public int MaxColors
        {
            get => _maxColors;
            set => SetProperty(ref _maxColors, value);
        }

        public bool RemoveWhite
        {
            get => _removeWhite;
            set => SetProperty(ref _removeWhite, value);
        }

        public bool DisableLayering
        {
            get => _disableLayering;
            set => SetProperty(ref _disableLayering, value);
        }

        public bool SmoothResizing
        {
            get => _smoothResizing;
            set => SetProperty(ref _smoothResizing, value);
        }

        public BitmapToSymbolArtConverterOptions GetOptions()
        {
            return new BitmapToSymbolArtConverterOptions()
            {
                ResizeImageHeight = _resizeImageHeight,
                MaxColors = _maxColors,
                SizeXOffset = _symbolSizeOffsetX,
                SizeYOffset = _symbolSizeOffsetY,
                RemoveWhite = _removeWhite,
                CenterYOffset = _centerYOffset,
                CenterXOffset = _centerXOffset,
                OffsetSizeYExponent = _offsetSizeYExponent,
                OffsetSizeXExponent = _offsetSizeXExponent,
                DisableLayering = _disableLayering,
                SmoothResizing = _smoothResizing
            };
        }
    }
}
