using CommunityToolkit.Mvvm.ComponentModel;
using OpenSAE.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace OpenSAE.Models
{
    public abstract class SymbolArtItemModel : ObservableObject
    {
        protected bool _isManipulating;
        protected bool _visible;
        protected string? _name;
        protected Point[] _temporaryVertices;
        protected IUndoModel _undoModel;

        protected SymbolArtItemModel(IUndoModel undoModel)
        {
            Children.CollectionChanged += (_, __) =>
            {
                OnChildrenChanged();
            };

            _undoModel = undoModel;
        }

        public event EventHandler? ChildrenChanged;

        public virtual string? Name
        {
            get => _name;
            set
            {
                SetPropertyWithUndo(_name, value, (x) =>
                {
                    if (SetProperty(ref _name, x))
                    {
                        OnPropertyChanged(nameof(DisplayName));
                    }
                }, $"Change {ItemTypeName} name");
            }
        }

        public abstract string ItemTypeName { get; }

        public virtual string DisplayName => Name ?? "[unnamed]";

        public virtual bool Visible
        {
            get => _visible;
            set => SetPropertyWithUndo(_visible, value, (x) => SetProperty(ref _visible, x), value ? $"Show {ItemTypeName}" : $"Hide {ItemTypeName}");
        }

        public abstract bool IsVisible { get; }

        protected void SetPropertyWithUndo<T>(T prop, T newValue, Action<T> setAction, string actionMessage, [CallerMemberName]string? propertyName = null)
        {
            T currentValue = prop;

            if (!Equals(currentValue, newValue))
            {
                setAction.Invoke(newValue);
                _undoModel.Set(
                    actionMessage, 
                    this,
                    propertyName!,
                    () => setAction(currentValue), 
                    () => setAction(newValue));
            }
        }

        /// <summary>
        /// Indicates if the vertices in this item should be enforced to the symbol art coordinate grid.
        /// If this value is false, vertex positions will not be rounded when displayed
        /// </summary>
        public virtual bool EnforceGridPositioning => true;

        public SymbolArtItemModel? Parent { get; set; }

        public abstract Point[] Vertices { get; set; }

        public abstract Point[] RawVertices { get; }

        /// <summary>
        /// Gets the currently committed vertices for the item. If the item is in manipulation mode
        /// will return the vertices before the manipulation begin. If not in manipulation mode
        /// returns the current vertices.
        /// </summary>
        public Point[] OriginalVertices => _isManipulating ? _temporaryVertices : Vertices;

        protected void OnChildrenChanged()
        {
            ChildrenChanged?.Invoke(this, EventArgs.Empty);
            Parent?.OnChildrenChanged();
        }

        public virtual void Delete()
        {
            if (Parent != null)
            {
                int indexInParent = IndexInParent;

                Parent.Children.Remove(this);

                _undoModel.Add($"Delete {ItemTypeName}", () => Parent.Children.Insert(indexInParent, this), () => Parent.Children.Remove(this));
            }
        }

        /// <summary>
        /// Commits the changes made in the current manipulation.
        /// </summary>
        public virtual void CommitManipulation()
        {
            if (_isManipulating)
            {
                _isManipulating = false;

                var previousVertices = _temporaryVertices;
                var newVertices = Vertices;

                _undoModel.Add($"Manipulate {ItemTypeName}", () => Vertices = previousVertices, () => Vertices = newVertices);
            }
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
        public virtual void StartManipulation()
        {
            if (!_isManipulating)
            {
                _isManipulating = true;
                _temporaryVertices = Vertices;
            }
        }

        public void Manipulate(Action<SymbolArtItemModel> action)
        {
            StartManipulation();
            action.Invoke(this);
            CommitManipulation();
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

        public IEnumerable<SymbolArtItemModel> GetAllItems()
        {
            yield return this;

            foreach (var child in Children)
            {
                foreach (var childItem in child.GetAllItems())
                {
                    yield return childItem;
                }
            }
        }

        public ObservableCollection<SymbolArtItemModel> Children { get; }
            = new();

        /// <summary>
        /// Gets or sets the position of the entire symbol. The origin of the position
        /// is the leftmost vertex
        /// </summary>
        public virtual Point Position
        {
            get => Vertices.GetMinBy(true);
            set
            {
                var points = Vertices;

                int minIndex = points.GetMinIndexBy(true);

                // find diff between previous min point and the new one
                var diff = value - points[minIndex];

                for (int i = 0; i < points.Length; i++)
                {
                    points[i] += diff;
                }

                Vertices = points;
                OnPropertyChanged();
            }
        }

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

        public virtual void FlipX()
        {
            Vertices = SymbolManipulationHelper.FlipX(Vertices);
        }

        public virtual void FlipY()
        {
            Vertices = SymbolManipulationHelper.FlipY(Vertices);
        }

        public virtual void Rotate(double angle)
        {
            Vertices = SymbolManipulationHelper.Rotate(Vertices, angle);
        }

        public virtual void TemporaryRotate(double angle)
        {
            StartManipulation();

            TemporaryRotate(angle, _temporaryVertices.GetCenter());
        }

        public virtual void TemporaryRotate(double angle, Point origin)
        {
            StartManipulation();

            Vertices = SymbolManipulationHelper.Rotate(_temporaryVertices!, origin, angle);
        }

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

        public static bool IsChildOfRecursive(SymbolArtItemModel childItem, SymbolArtItemModel parentItem)
        {
            if (childItem.Parent == parentItem)
                return true;

            if (childItem.Parent == null)
                return false;

            return IsChildOfRecursive(childItem.Parent, parentItem);
        }

        public abstract void SetVertex(int vertexIndex, Point point);

        public abstract void ResizeFromVertex(int vertexIndex, Point point);
    }
}
