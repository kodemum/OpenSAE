using OpenSAE.Core;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OpenSAE.Models
{
    public class SymbolArtLayerModel : SymbolArtItemModel
    {
        private readonly SymbolArtLayer _layer;
        private readonly SymbolArtItemModel _parent;

        public SymbolArtLayerModel(SymbolArtLayer layer, SymbolArtItemModel parent)
        {
            _layer = layer;
            _parent = parent;
        }

        public override string? Name
        {
            get => _layer.Name;
            set
            {
                _layer.Name = value;
                OnPropertyChanged();
            }
        }

        public override bool Visible
        {
            get => _layer.Visible;
            set
            {
                _layer.Visible = value;
                OnPropertyChanged();
            }
        }

        public override bool IsVisible => _parent.IsVisible && Visible;

        public string? SymbolPackUri => SymbolUtil.GetSymbolPackUri(_layer.Type);

        public double Alpha
        {
            get => _layer.Alpha;
            set
            {
                _layer.Alpha = value;
                OnPropertyChanged();
            }
        }

        public Color Color
        {
            get
            {
                var color = (Color)ColorConverter.ConvertFromString(_layer.Color ?? "#ffffff");

                color.A = (byte)Math.Round(_layer.Alpha * 255);

                return color;
            }
            set
            {
                _layer.Color = string.Format("#{0:x2}{1:x2}{2:x2}", value.R, value.G, value.B);
                OnPropertyChanged();

                Alpha = Math.Round((double)value.A / 255 * 7) / 7;
            }
        }

        public IEnumerable<Point3D> Points
        {
            get
            {
                yield return new Point3D(_layer.Lbx, -_layer.Lby, 0);
                yield return new Point3D(_layer.Ltx, -_layer.Lty, 0);
                yield return new Point3D(_layer.Rbx, -_layer.Rby, 0);
                yield return new Point3D(_layer.Rtx, -_layer.Rty, 0);
            }
        }
    }
}
