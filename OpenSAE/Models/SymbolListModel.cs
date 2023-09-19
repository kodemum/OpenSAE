using CommunityToolkit.Mvvm.ComponentModel;
using OpenSAE.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Models
{
    public class SymbolListModel : ObservableObject
    {
        private double _itemSize
            = 64;

        public ObservableCollection<Symbol> Symbols { get; }

        public double ItemSize
        {
            get => _itemSize;
            set => SetProperty(ref _itemSize, value);
        }

        public SymbolListModel(IEnumerable<Symbol>? symbols = null)
        {
            Symbols = new ObservableCollection<Symbol>(symbols ?? SymbolUtil.List);
        }
    }
}
