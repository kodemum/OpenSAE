using OpenSAE.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OpenSAE.Converters
{
    /// <summary>
    /// Converter to multiply numbers
    /// </summary>
    internal class MultiplyNumberConverter : IMultiValueConverter
    {
        /// <summary>
        /// Takes two doubles as values and multiplies them.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                throw new ArgumentException("Converter must have two arguments", nameof(values));

            if (TryGetDouble(values[1], out double value1) && TryGetDouble(values[1], out double value2))
            {
                return value1 * value2;
            }
            else if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
            {
                return DependencyProperty.UnsetValue;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private static bool TryGetDouble(object input, out double result)
        {
            if (input is int i)
            {
                result = i;
                return true;
            }
            else if (input is double d)
            {
                result = d;
                return true;
            }
            else if (input is string s)
            {
                return double.TryParse(s, out result);
            }

            result = -1;
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
