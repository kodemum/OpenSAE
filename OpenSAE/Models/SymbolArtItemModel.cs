using CommunityToolkit.Mvvm.ComponentModel;
using OpenSAE.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace OpenSAE.Models
{
    public abstract class SymbolArtItemModel : ObservableObject
    {
        protected bool _isManipulating;
        protected Point[] _temporaryVertices;

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

        public virtual string DisplayName => Name ?? "[unnamed]";

        public abstract bool Visible { get; set; }

        public abstract bool IsVisible { get; }

        public SymbolArtItemModel? Parent { get; set; }

        public abstract Point[] Vertices { get; set; }

        /// <summary>
        /// Gets the currently committed vertices for the item. If the item is in manipulation mode
        /// will return the vertices before the manipulation begin. If not in manipulation mode
        /// returns the current vertices.
        /// </summary>
        public Point[] OriginalVertices => _isManipulating ? _temporaryVertices : Vertices;

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

        /// <summary>
        /// Commits the changes made in the current manipulation.
        /// </summary>
        public virtual void CommitManipulation()
        {
            _isManipulating = false;
        }

        /// <summary>
        /// Discards any non-committed manipulations performed on the item.
        /// </summary>
        public virtual void DiscardManipulation()
        {
            if (_isManipulating)
            {
                _isManipulating = false;
                Vertices = _temporaryVertices;
            }
        }

        /// <summary>
        /// Specify that a manipulation should be started if one is not already in progress.
        /// Enabling this saves a copy of the current vertices so that operations can be performed
        /// on these rather than successively on the same vertices. This helps to avoid rounding errors
        /// if many operations are performed such as when drag-rotating an item.
        /// </summary>
        protected void StartManipulation()
        {
            if (!_isManipulating)
            {
                _isManipulating = true;
                _temporaryVertices = Vertices;
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

        public abstract Point Position { get; set; }

        public abstract bool ShowBoundingVertices { get; set; }

        public abstract double Alpha { get; set; }

        public abstract Color Color { get; set; }

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

        public abstract void SetVertex(int vertexIndex, Point point);

        public abstract void TemporaryRotate(double angle);

        public abstract void ResizeFromVertex(int vertexIndex, Point point);
    }
}
