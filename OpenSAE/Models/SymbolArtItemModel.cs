using CommunityToolkit.Mvvm.ComponentModel;
using OpenSAE.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenSAE.Models
{
    public abstract class SymbolArtItemModel : ObservableObject
    {
        protected SymbolArtItemModel()
        {
            Children.CollectionChanged += (_, __) =>
            {
                OnChildrenChanged();
                Parent?.OnChildrenChanged();
            };
        }

        public event EventHandler? ChildrenChanged;

        public abstract string? Name { get; set; }

        public abstract bool Visible { get; set; }

        public abstract bool IsVisible { get; }

        public SymbolArtItemModel? Parent { get; set; }

        protected void OnChildrenChanged()
        {
            ChildrenChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Delete()
        {
            if (Parent != null)
            {
                Parent.Children.Remove(this);
            }
        }

        public IEnumerable<SymbolArtLayerModel> GetAllLayers()
        {
            if (this is SymbolArtLayerModel layer)
            {
                yield return layer;
            }
            else
            {
                foreach (var child in Children)
                {
                    if (child is SymbolArtLayerModel childLayer)
                    {
                        yield return childLayer;
                    }
                    else if (child is SymbolArtGroupModel group)
                    {
                        foreach (var groupLayer in group.GetAllLayers())
                        {
                            yield return groupLayer;
                        }
                    }
                }
            }
        }

        public ObservableCollection<SymbolArtItemModel> Children { get; }
            = new();

        public abstract SymbolArtItemModel Duplicate(SymbolArtItemModel parent);

        public abstract SymbolArtItem ToSymbolArtItem();

        public abstract void FlipX();

        public abstract void FlipY();

        public abstract void Rotate(double angle);

        public static bool IsChildOf(object childItem, object parentItem)
        {
            if (childItem is not SymbolArtItemModel childModel)
            {
                throw new Exception();
            }

            if (parentItem == null)
                return false;

            return childModel.Parent == parentItem;
        }
    }
}
