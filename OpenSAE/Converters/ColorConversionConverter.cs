using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OpenSAE.Converters
{
    internal class ColorConversionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;

            if (value is not Color color)
                throw new ArgumentException(nameof(value));

            return Core.SymbolArtColorHelper.ApplyCurve(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;

            if (value is not Color color)
                throw new ArgumentException(nameof(value));

            return Core.SymbolArtColorHelper.RemoveCurve(color);
        }
    }
}
