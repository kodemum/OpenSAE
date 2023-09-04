using CommunityToolkit.Mvvm.ComponentModel;
using OpenSAE.Core;
using OpenSAE.Core.BitmapConverter;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace OpenSAE.Models
{
    internal class BitmapToSymbolArtConverterOptionsViewModel : ObservableObject
    {
        private int _resizeImageHeight;
        private bool _respectEdges;
        private double _symbolOpacity;
        private int _shapesPerStep;
        private int _maxSymbolCount;
        private bool _includeBackground;
        private Color _backgroundColor;

        private int _mutationsPerStep;
        private List<ShapeType> _shapeTypes;

        public BitmapToSymbolArtConverterOptionsViewModel()
        {
            var defaultOptions = new BitmapToSymbolArtConverterOptions();
            _backgroundColor = defaultOptions.BackgroundColor;
            _includeBackground = defaultOptions.IncludeBackground;
            _maxSymbolCount = defaultOptions.MaxSymbolCount;
            _mutationsPerStep = defaultOptions.MutationsPerStep;
            _resizeImageHeight = defaultOptions.ResizeImageHeight;
            _respectEdges = defaultOptions.RespectEdges;
            _shapesPerStep = defaultOptions.ShapesPerStep;
            _shapeTypes = defaultOptions.ShapeTypes.ToList();
            _symbolOpacity = defaultOptions.SymbolOpacity;
        }

        public int ResizeImageHeight
        {
            get => _resizeImageHeight;
            set => SetProperty(ref _resizeImageHeight, value);
        }

        public bool RespectEdges
        {
            get => _respectEdges;
            set => SetProperty(ref _respectEdges, value);
        }

        public double SymbolOpacity
        {
            get => _symbolOpacity;
            set => SetProperty(ref _symbolOpacity, value);
        }

        public int ShapesPerStep
        {
            get => _shapesPerStep;
            set => SetProperty(ref _shapesPerStep, value);
        }

        public int MutationsPerStep
        {
            get => _mutationsPerStep;
            set => SetProperty(ref _mutationsPerStep, value);
        }

        public bool EnableRectangleShape
        {
            get => HasShapeType(ShapeType.Rectangle);
            set => SetShapeType(ShapeType.Rectangle, value);
        }

        public bool EnableRotatedRectangleShape
        {
            get => HasShapeType(ShapeType.Rotated_Rectangle);
            set => SetShapeType(ShapeType.Rotated_Rectangle, value);
        }

        public bool EnableEllipseShape
        {
            get => HasShapeType(ShapeType.Ellipse);
            set => SetShapeType(ShapeType.Ellipse, value);
        }

        public bool EnableRotatedEllipseShape
        {
            get => HasShapeType(ShapeType.Rotated_Ellipse);
            set => SetShapeType(ShapeType.Rotated_Ellipse, value);
        }

        public bool EnableCircleShape
        {
            get => HasShapeType(ShapeType.Circle);
            set => SetShapeType(ShapeType.Circle, value);
        }

        public int MaxSymbolCount
        {
            get => _maxSymbolCount;
            set => SetProperty(ref _maxSymbolCount, value);
        }

        public bool IncludeBackground
        {
            get => _includeBackground;
            set => SetProperty(ref _includeBackground, value);
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }

        private bool HasShapeType(ShapeType type) => _shapeTypes.Contains(type);
        
        private void SetShapeType(ShapeType type, bool isSet, [CallerMemberName]string? propertyName = null)
        {
            if (isSet && !HasShapeType(type))
                _shapeTypes.Add(type);
            else if (!isSet)
                _shapeTypes.Remove(type);

            OnPropertyChanged(propertyName);
        }

        public BitmapToSymbolArtConverterOptions GetOptions()
        {
            return new BitmapToSymbolArtConverterOptions()
            {
                BackgroundColor = _backgroundColor,
                IncludeBackground = _includeBackground,
                MaxSymbolCount = _maxSymbolCount,
                MutationsPerStep = _mutationsPerStep,
                ResizeImageHeight = _resizeImageHeight,
                RespectEdges = _respectEdges,
                ShapesPerStep = _shapesPerStep,
                ShapeTypes = _shapeTypes.ToArray(),
                SymbolOpacity = _symbolOpacity,
            };
        }
    }
}
