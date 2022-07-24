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

        public int IndexInParent
        {
            get => Parent?.Children.IndexOf(this) ?? 0;
            set 
            {
                if (Parent != null)
                {
                    Parent.Children.Move(IndexInParent, value);
                }
            }
        }

        public void MoveUp()
        {
            if (IndexInParent > 0)
            {
                IndexInParent--;
            }
        }

        public void MoveDown()
        {
            if (IndexInParent < Parent?.Children.Count - 1)
            {
                IndexInParent++;
            }
        }

        public void MoveTo(SymbolArtItemModel target)
        {
            if (Parent == null)
                return;

            if (target is SymbolArtGroupModel group)
            {
                Parent.Children.Remove(this);
                group.Children.Insert(0, this);
                Parent = group;
            }
            else if (target.Parent == Parent)
            {
                // already in same group
                IndexInParent = target.IndexInParent;
            }
            else if (target.Parent != null)
            {
                // move to group at same index as target
                Parent.Children.Remove(this);
                target.Parent.Children.Insert(target.IndexInParent, this);
                Parent = target.Parent;
            }
        }

        public void CopyTo(SymbolArtItemModel target)
        {
            if (Parent == null)
                return;

            if (target is SymbolArtGroupModel group)
            {
                group.Children.Insert(0, Duplicate(group));
            }
            else if (target.Parent != null)
            {
                // copy to group at same index as target
                target.Parent.Children.Insert(target.IndexInParent, Duplicate(target.Parent));
            }
        }

        /// <summary>
        /// Gets the maximum layer index that can be found in the tree of items
        /// </summary>
        public virtual int GetMaxLayerIndex()
            => Parent?.GetMaxLayerIndex() ?? 0;

        /// <summary>
        /// Checks if the specified point (symbol art coordinates) is inside of the item.
        /// </summary>
        /// <param name="testPoint"></param>
        /// <returns></returns>
        public bool IsPointInside(Point testPoint)
        {
            var polygon = Vertices;

            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }

            return result;
        }

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
