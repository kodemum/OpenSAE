using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Models
{
    public class BitmapConversionRequestedEventArgs : EventArgs
    {
        public BitmapConversionRequestedEventArgs(BitmapConverterModel model)
        {
            Model = model;
        }

        public BitmapConverterModel Model { get; }
    }
}
