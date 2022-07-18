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
    internal class LayerPointCollectionConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SymbolArtPoint[] points)
            {
                // as this converter is used for calculating display, we need to round the vertices
                return new PointCollection(new Point[]
                {
                    points[0].Round(),
                    points[1].Round(),
                    points[3].Round(),
                    points[2].Round(),
                    points[0].Round()
                });
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
