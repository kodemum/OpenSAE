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
            if (value is SymbolArtLayerModel layer)
            {
                return new PointCollection(new Point[]
                {
                    layer.Vertex1,
                    layer.Vertex2,
                    layer.Vertex4,
                    layer.Vertex3,
                    layer.Vertex1
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
