using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OpenSAE.Converters
{
    /// <summary>
    /// Multiconverter used for converting positions from symbol art coordinates to
    /// screen coordinates.
    /// </summary>
    internal class RelativeNumberConverter : IMultiValueConverter
    {
        /// <summary>
        /// Reference value to compare with extent given as argument
        /// </summary>
        public double ReferenceExtent { get; set; }

        /// <summary>
        /// If set, will divide the result by this
        /// </summary>
        public double? DivideBy { get; set; }

        /// <summary>
        /// If set, will divide any additional values by this
        /// </summary>
        public double? DivideAdditionalValuesBy { get; set; }

        /// <summary>
        /// Takes at least two arguments as values; the first being an extent that is divided by the
        /// value specified in <see cref="ReferenceExtent"/> and the second being the actual target value.
        /// Additional values may be specified after the target value and these will be added as well.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                throw new ArgumentException("Converter must have at least two arguments", nameof(values));

            if (values[0] is double extent && TryGetDouble(values[1], out double input))
            {
                for (int i = 2; i < values.Length; i++)
                {
                    if (TryGetDouble(values[i], out double inputAdd))
                    {
                        input += DivideAdditionalValuesBy.HasValue ? inputAdd / DivideAdditionalValuesBy.Value : inputAdd;
                    }
                }

                if (DivideBy != null)
                {
                    input /= DivideBy.Value;
                }

                return input * extent / ReferenceExtent;
            }
            else if (values[1] == DependencyProperty.UnsetValue)
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
