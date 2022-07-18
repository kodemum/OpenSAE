using OpenSAE.Core;
using System;

namespace OpenSAE.Models
{
    public class SymbolArtGroupModel : SymbolArtItemModel
    {
        private readonly ISymbolArtGroup _group;

        public SymbolArtGroupModel(ISymbolArtGroup group, SymbolArtItemModel parent)
        {
            _group = group;
            Parent = parent;

            foreach (var item in _group.Children)
            {
                if (item is ISymbolArtGroup subGroup)
                {
                    Children.Add(new SymbolArtGroupModel(subGroup, this));
                }
                else if (item is SymbolArtLayer layer)
                {
                    Children.Add(new SymbolArtLayerModel(layer, this));
                }
                else
                {
                    throw new Exception($"Item of unknown type {item.GetType().Name} found in symbol art group");
                }
            }
        }

        public override string? Name
        {
            get => _group.Name;
            set
            {
                _group.Name = value;
                OnPropertyChanged();
            }
        }

        public override bool Visible
        {
            get => _group.Visible;
            set
            {
                _group.Visible = value;
                OnPropertyChanged();
            }
        }

        public override bool IsVisible => Parent!.IsVisible && Visible;
    }


}
