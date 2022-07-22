using OpenSAE.Core;
using OpenSAE.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OpenSAE.Converters
{
    internal class LayerPointCollectionConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                throw new ArgumentException();

            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
            {
                return DependencyProperty.UnsetValue;
            }

            if (values.Length == 2 && values[0] is double scale && values[1] is Point[] points)
            {
                // as this converter is used for calculating display, we need to round the vertices
                return new PointCollection(new Point[]
                {
                    points[0].Multiply(scale).Round(),
                    points[1].Multiply(scale).Round(),
                    points[3].Multiply(scale).Round(),
                    points[2].Multiply(scale).Round(),
                    points[0].Multiply(scale).Round()
                });
            }

            throw new ArgumentException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
