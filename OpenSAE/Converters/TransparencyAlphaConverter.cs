using System;
using System.Globalization;
using System.Windows.Data;

namespace OpenSAE.Converters
{
    internal class TransparencyAlphaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return Math.Round(d * 7);
            }

            throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return d / 7;
            }

            throw new ArgumentException();
        }
    }
}
