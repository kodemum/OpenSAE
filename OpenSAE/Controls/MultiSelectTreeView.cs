﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Diagnostics;

namespace OpenSAE.Controls
{
    public delegate bool IsChildOfPredicate(object nodeA, object nodeB);

    /// <summary>
    /// Extended TreeView control that supports multi-select and two-way data binding
    /// for the selected items.
    /// </summary>
    public sealed class MultiSelectTreeView : TreeView
    {
        private TreeViewItem? _lastItemSelected;
        private bool _isUpdating;

        public static readonly DependencyProperty IsItemSelectedProperty =
            DependencyProperty.RegisterAttached("IsItemSelected", typeof(bool), typeof(MultiSelectTreeView));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(IEnumerable),
                typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedItemsChanged));

        public static readonly DependencyProperty HierarchyPredicateProperty =
            DependencyProperty.Register(nameof(HierarchyPredicate), typeof(IsChildOfPredicate),
                typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ExpandSelectedProperty =
            DependencyProperty.Register(nameof(ExpandSelected), typeof(bool),
                typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(false));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MultiSelectTreeView view || view._isUpdating)
                return;

            view._isUpdating = true;

            var items = GetTreeViewItems(view, true).ToList();
            items.ForEach(x => SetIsItemSelected(x, false));

            if (e.NewValue is IEnumerable newItems)
            {
                List<TreeViewItem> itemContainers = new();
                HashSet<object> itemSet = new();

                foreach (var item in newItems)
                {
                    itemSet.Add(item);
                }

                view.ExpandToAndSelectItems(view, itemSet, itemContainers);

                // if the normal selection of the TreeView isn't one of the target items
                // then change it to the last one to ensure they're not out of sync
                //
                // if we don't do this, the item the TreeView considers selected internally cannot be selected
                // by clicking on it in the control
                if (!itemContainers.Any(x => x.DataContext == view.SelectedItem) && itemContainers.Count > 0)
                {
                    itemContainers[^1].IsSelected = true;
                }
            }

            view._isUpdating = false;
        }

        public static void SetIsItemSelected(UIElement element, bool value)
        {
            element.SetValue(IsItemSelectedProperty, value);
        }

        public static bool GetIsItemSelected(UIElement element)
        {
            return (bool)element.GetValue(IsItemSelectedProperty);
        }

        private bool ExpandToAndSelectItems(ItemsControl source, HashSet<object> targetItems, List<TreeViewItem> items)
        {
            foreach (var item in source.Items)
            {
                if (source.ItemContainerGenerator.ContainerFromItem(item) is not TreeViewItem container)
                {
                    // assuming our expansion and building of container elements works correctly,
                    // we should always be able to find the container element
                    // but we don't want to throw an exception should it fail
                    continue;
                }

                if (targetItems.Contains(item))
                {
                    SetIsItemSelected(container, true);

                    if (ExpandSelected)
                        container.IsExpanded = true;

                    container.BringIntoView();

                    items.Add(container);
                    targetItems.Remove(item);

                    if (targetItems.Count == 0)
                    {
                        return true;
                    }

                    continue;
                }

                bool isParentOfModel = HierarchyPredicate == null || targetItems.Any(x => HierarchyPredicate.Invoke(x, item));
                if (isParentOfModel)
                {
                    container.IsExpanded = true;

                    if (container.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.NotStarted)
                    {
                        // required to build container elements
                        UpdateLayout();
                    }

                    if (ExpandToAndSelectItems(container, targetItems, items))
                        return true;
                }
            }

            return false;
        }

        private static bool IsCtrlPressed => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

        private static bool IsShiftPressed => Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

        public IEnumerable SelectedItems
        {
            get => (IEnumerable)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public IsChildOfPredicate HierarchyPredicate
        {
            get => (IsChildOfPredicate)GetValue(HierarchyPredicateProperty);
            set => SetValue(HierarchyPredicateProperty, value);
        }

        public bool ExpandSelected
        {
            get => (bool)GetValue(ExpandSelectedProperty);
            set => SetValue(ExpandSelectedProperty, value);
        }

        /// <summary>
        /// We use the normal item selected event to add the new selection to our list of selected items
        /// (and possibly clear it, depending on modifier keys held)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);

            if (SelectedItem == null || _isUpdating)
                return;

            var item = ContainerFromItemRecursive(ItemContainerGenerator, SelectedItem);
            if (item != null)
            {
                SelectedItemChangedInternal(item);
            }
        }

        private static TreeViewItem? ContainerFromItemRecursive(ItemContainerGenerator root, object item)
        {
            var treeViewItem = root.ContainerFromItem(item) as TreeViewItem;
            if (treeViewItem != null)
                return treeViewItem;

            foreach (var subItem in root.Items)
            {
                treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem;

                if (treeViewItem != null)
                {
                    var target = ContainerFromItemRecursive(treeViewItem.ItemContainerGenerator, item);
                    if (target != null)
                        return target;
                }
            }

            return null;
        }

        private void SelectedItemChangedInternal(TreeViewItem item)
        {
            // Clear all previous selected item states if ctrl is NOT being held down
            if (!IsCtrlPressed)
            {
                var items = GetTreeViewItems(this, true);
                foreach (TreeViewItem treeViewItem in items)
                {
                    SetIsItemSelected(treeViewItem, false);
                }
            }

            // Is this an item range selection?
            if (IsShiftPressed && _lastItemSelected != null)
            {
                var items = GetTreeViewItemRange(_lastItemSelected, item);
                if (items.Count > 0)
                {
                    foreach (var treeViewItem in items)
                    {
                        SetIsItemSelected(treeViewItem, true);
                    }
                }
            }
            // Otherwise, individual selection
            else
            {
                SetIsItemSelected(item, !GetIsItemSelected(item));
                _lastItemSelected = item;
            }

            RefreshSelectedItems();
        }

        private void RefreshSelectedItems()
        {
            var items = GetTreeViewItems(this, false)
                .Where(GetIsItemSelected)
                .Select(x => x.DataContext)
                .ToList();

            _isUpdating = true;
            SelectedItems = items;
            _isUpdating = false;
        }

        private static List<TreeViewItem> GetTreeViewItems(ItemsControl parentItem, bool includeCollapsedItems, List<TreeViewItem>? itemList = null)
        {
            itemList ??= new List<TreeViewItem>();

            for (var index = 0; index < parentItem.Items.Count; index++)
            {
                if (parentItem.ItemContainerGenerator.ContainerFromIndex(index) is not TreeViewItem item)
                {
                    continue;
                }

                itemList.Add(item);

                if (includeCollapsedItems || item.IsExpanded)
                {
                    GetTreeViewItems(item, includeCollapsedItems, itemList);
                }
            }
            return itemList;
        }

        private List<TreeViewItem> GetTreeViewItemRange(TreeViewItem start, TreeViewItem end)
        {
            var items = GetTreeViewItems(this, false);

            var startIndex = items.IndexOf(start);
            var endIndex = items.IndexOf(end);
            var rangeStart = startIndex > endIndex || startIndex == -1 
                ? endIndex 
                : startIndex;
            var rangeCount = startIndex > endIndex 
                ? startIndex - endIndex + 1 
                : endIndex - startIndex + 1;

            if (startIndex == -1 && endIndex == -1)
                rangeCount = 0;
            else if (startIndex == -1 || endIndex == -1)
                rangeCount = 1;

            return rangeCount > 0 ? items.GetRange(rangeStart, rangeCount) : new List<TreeViewItem>();
        }
    }
}
