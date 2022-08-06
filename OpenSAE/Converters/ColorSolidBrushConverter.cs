using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OpenSAE.Converters
{
    internal class ColorSolidBrushConverter : IValueConverter
    {
        public bool UseToneCurve { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                if (UseToneCurve)
                    return new SolidColorBrush(Core.SymbolArtColorHelper.ApplyCurve(color));
                else
                    return new SolidColorBrush(color);
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
