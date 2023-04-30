using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace OpenSAE.Models
{
    /// <summary>
    /// Represents a collection of symbol art items that can be manipulated together as they were a group.
    /// This collection can be manipulated, but it should _not_ be added to a symbol art or have items added to it.
    /// </summary>
    public class SymbolArtItemCollection : SymbolArtGroupModel
    {
        public const string CollectionIdentifier = "//virtual group//";

        public override string ItemTypeName => $"{GetAllLayers().Count()} symbols";

        /// <summary>
        /// The first item in this collection
        /// </summary>
        public SymbolArtItemModel FirstItem { get; }

        protected SymbolArtItemCollection(SymbolArtItemModel firstItem) 
            : base(firstItem.Undo, CollectionIdentifier, true, firstItem.Parent)
        {
            FirstItem = firstItem;
        }

        public override string? Name 
        {
            get => $"[{GetAllLayers().Count()} symbols]";
            set => base.Name = value; 
        }

        /// <summary>
        /// Override this to the first item in the collection to avoid exceptions due to the collection
        /// not existing in the item specified as its parent (which is <see cref="FirstItem"/>)
        /// </summary>
        public override int IndexInParent
        {
            get => FirstItem.IndexInParent;
            set => throw new InvalidOperationException("A symbol art item collection cannot be moved");
        }

        public override bool Visible
        {
            get => FirstItem.Visible;
            set
            {
                if (Visible != value)
                {
                    using var scope = _undoModel.StartAggregateScope($"{(value ? "Show" : "Hide")} {ItemTypeName}", this, "hide");

                    foreach (var item in Children)
                    {
                        item.Visible = value;
                    }
                }
            }
        }

        public override void Delete()
        {
            using var scope = _undoModel.StartAggregateScope($"Delete {ItemTypeName}");

            foreach (var item in Children)
            {
                item.Delete();
            }
        }

        public override void MoveUp()
        {
            using var scope = _undoModel.StartAggregateScope($"Reorder {ItemTypeName}");

            foreach (var item in Children)
            {
                item.MoveUp();
            }
        }

        public override void MoveDown()
        {
            using var scope = _undoModel.StartAggregateScope($"Reorder {ItemTypeName}");

            foreach (var item in Children)
            {
                item.MoveDown();
            }
        }

        /// <summary>
        /// Creates a new item collection that contains the specified items. At least one item must be specified.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static SymbolArtItemCollection Create(IEnumerable<SymbolArtItemModel> source)
        {
            var items = source.ToArray();
            if (items.Length == 0)
                throw new ArgumentException("Collection cannot be empty", nameof(source));

            var group = new SymbolArtItemCollection(items[0]);

            foreach (var item in items)
            {
                group.Children.Add(item);
            }

            return group;
        }
    }
}
