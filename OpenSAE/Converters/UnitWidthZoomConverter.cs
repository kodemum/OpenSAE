using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OpenSAE.Converters
{
    internal class UnitWidthZoomConverter : IValueConverter
    {
        public double ValueAtZero { get; set; }

        public double ValueAtHalf { get; set; }

        public double ValueAtFull { get; set; }

        private (double a, double b, double c) GetConstants()
        {
            return (
                (ValueAtZero * ValueAtFull - Math.Pow(ValueAtHalf, 2)) / (ValueAtZero - ValueAtHalf * 2 + ValueAtFull),
                Math.Pow(ValueAtHalf - ValueAtZero, 2) / (ValueAtZero - ValueAtHalf * 2 + ValueAtFull),
                2 * Math.Log((ValueAtFull - ValueAtHalf) / (ValueAtHalf - ValueAtZero))
            );
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double input)
            {
                (double a, double b, double c) = GetConstants();

                return Math.Log((input - a) / b) / c;
            }
            else
            {
                throw new ArgumentException(null, nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double input)
            {
                (double a, double b, double c) = GetConstants();

                return a + b * Math.Exp(c * input);
            }
            else
            {
                throw new ArgumentException(null, nameof(value));
            }
        }
    }
}
