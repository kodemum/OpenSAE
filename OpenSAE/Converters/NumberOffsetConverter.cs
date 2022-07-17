using System;
using System.Globalization;
using System.Windows.Data;

namespace OpenSAE.Converters
{
    internal class NumberOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is short && parameter is string arg && short.TryParse(arg, out short offset))
            {
                return (short)value + offset;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
