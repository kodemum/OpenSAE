using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace OpenSAE.Converters
{
    internal class Point3DConverter : IValueConverter
    {
        public double Z { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Point point)
            {
                return new Point3D(-point.X, point.Y, Z);
            } 
            else if (value == DependencyProperty.UnsetValue)
            {
                return DependencyProperty.UnsetValue;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
