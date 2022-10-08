using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenSAE.Models
{
    /// <summary>
    /// Edit view for modifying hue/saturation/brightness/contrast for all symbols in an item
    /// </summary>
    public class SymbolArtColorAdjustmentModel : EditViewModel
    {
        private const string UndoIndentifier = "AdjustHueSaturation";

        private readonly IUndoModel _undoModel;
        private readonly Dictionary<SymbolArtLayerModel, Color> _originalColors = new();

        private int _hue;
        private int _saturation;
        private int _brightness;
        private int _contrast;

        public int Hue
        {
            get => _hue;
            set => SetRefreshProperty(ref _hue, value);
        }

        public int Saturation
        {
            get => _saturation;
            set => SetRefreshProperty(ref _saturation, value);
        }

        public int Brightness
        {
            get => _brightness;
            set => SetRefreshProperty(ref _brightness, value);
        }

        public int Contrast
        {
            get => _contrast;
            set => SetRefreshProperty(ref _contrast, value);
        }

        public override string Title => "Adjust color / brightness / contrast";

        public override string Subtitle => $"Affects {_originalColors.Count} symbols";

        public SymbolArtColorAdjustmentModel(IUndoModel undoModel, SymbolArtItemModel target)
        {
            _undoModel = undoModel;
            _hue = 0;
            _saturation = 0;
            _brightness = 0;
            _contrast = 0;

            _originalColors = target.GetAllLayers().ToDictionary(x => x, x => x.Color);
        }

        private bool SetRefreshProperty<T>(ref T prop, T value, [CallerMemberName]string? propertyName = null)
        {
            bool different = SetProperty(ref prop, value, propertyName);

            if (different)
            {
                ApplyModifications();
            }

            return different;
        }

        private void ApplyModifications()
        {
            if (Brightness == 0 && Saturation == 0 && Hue == 0 && Contrast == 0)
            {
                return;
            }

            using var scope = _undoModel.StartAggregateScope("Adjust hue/saturation", this, UndoIndentifier);

            foreach (var item in _originalColors)
            {
                item.Key.Color = ApplyTransformation(item.Value);
            }
        }

        private Color ApplyTransformation(Color color)
        {
            var hslColor = new HslColor(color);

            double h = hslColor.h, s = hslColor.s, l = hslColor.l;

            if (Brightness != 0)
            {
                l = Math.Clamp(l + (Brightness / 100.0), 0, 1);
            }

            if (Saturation != 0)
            {
                s = Math.Clamp(s + (Saturation / 100.0), 0, 1);
            }

            if (Hue != 0)
            {
                h += Hue;
            }

            var rgb = new HslColor(h, s, l, hslColor.a).ToRgb();

            if (Contrast != 0)
            {
                var factor = 259 * (Contrast * 1.28 + 255) / (255 * (259 - Contrast * 1.28));

                return Color.FromRgb(
                    (byte)Math.Clamp(factor * (rgb.R - 128) + 128, 0, 255),
                    (byte)Math.Clamp(factor * (rgb.G - 128) + 128, 0, 255),
                    (byte)Math.Clamp(factor * (rgb.B - 128) + 128, 0, 255)
                );
            } 
            else
            {
                return rgb;
            }
        }

        public override void ApplyChanges()
        {
        }

        public override void Cancel()
        {
            _undoModel.UndoAndRemoveSpecific(this, UndoIndentifier);
        }
    }
}
