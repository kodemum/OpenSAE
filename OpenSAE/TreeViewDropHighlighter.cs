﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenSAE
{
    /// <summary>
    /// Implements an attached property used for styling TreeViewItems when they're a possible drop target.
    /// Source: https://stackoverflow.com/questions/639884/highlight-treeview-item-being-dragged-over
    /// </summary>
    public static class TreeViewDropHighlighter
    {
        #region private variables
        /// <summary>
        /// the TreeViewItem that is the current drop target
        /// </summary>
        private static TreeViewItem? _currentItem = null;

        /// <summary>
        /// The last TreeViewItem drop target that was not expanded.
        /// </summary>
        private static TreeViewItem? _lastUnexpanded = null;

        /// <summary>
        /// The last time the last TreeViewItem that was not expanded changed.
        /// </summary>
        private static DateTime _lastUnexpandedUpdated;

        /// <summary>
        /// Indicates whether the current TreeViewItem is a possible
        /// drop target
        /// </summary>
        private static bool _dropPossible;
        #endregion

        #region IsPossibleDropTarget
        /// <summary>
        /// Property key (since this is a read-only DP) for the IsPossibleDropTarget property.
        /// </summary>
        private static readonly DependencyPropertyKey IsPossibleDropTargetKey =
                                    DependencyProperty.RegisterAttachedReadOnly(
                                        "IsPossibleDropTarget",
                                        typeof(bool),
                                        typeof(TreeViewDropHighlighter),
                                        new FrameworkPropertyMetadata(null,
                                            new CoerceValueCallback(CalculateIsPossibleDropTarget)));


        /// <summary>
        /// Dependency Property IsPossibleDropTarget.
        /// Is true if the TreeViewItem is a possible drop target (i.e., if it would receive
        /// the OnDrop event if the mouse button is released right now).
        /// </summary>
        public static readonly DependencyProperty IsPossibleDropTargetProperty = IsPossibleDropTargetKey.DependencyProperty;

        /// <summary>
        /// Getter for IsPossibleDropTarget
        /// </summary>
        public static bool GetIsPossibleDropTarget(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsPossibleDropTargetProperty);
        }

        /// <summary>
        /// Coercion method which calculates the IsPossibleDropTarget property.
        /// </summary>
        private static object CalculateIsPossibleDropTarget(DependencyObject item, object value)
        {
            if ((item == _currentItem) && (_dropPossible))
                return true;
            else
                return false;
        }
        #endregion

        /// <summary>
        /// Initializes the <see cref="TreeViewDropHighlighter"/> class.
        /// </summary>
        static TreeViewDropHighlighter()
        {
            // Get all drag enter/leave events for TreeViewItem.
            EventManager.RegisterClassHandler(typeof(TreeViewItem),
                                      TreeViewItem.PreviewDragEnterEvent,
                                      new DragEventHandler(OnDragEvent), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem),
                                      TreeViewItem.PreviewDragLeaveEvent,
                                      new DragEventHandler(OnDragLeave), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem),
                                      TreeViewItem.PreviewDragOverEvent,
                                      new DragEventHandler(OnDragEvent), true);

            EventManager.RegisterClassHandler(typeof(TreeViewItem),
                                      TreeViewItem.PreviewDropEvent,
                                      new DragEventHandler(OnDragLeave), true);
        }

        #region event handlers
        /// <summary>
        /// Called when an item is dragged over the TreeViewItem.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        static void OnDragEvent(object sender, DragEventArgs args)
        {
            lock (IsPossibleDropTargetProperty)
            {
                TreeViewItem? tvi = sender as TreeViewItem;

                _dropPossible = false;

                if (_currentItem != null)
                {
                    // Tell the item that previously had the mouse that it no longer does.
                    DependencyObject oldItem = _currentItem;
                    _currentItem = null;
                    oldItem.InvalidateProperty(IsPossibleDropTargetProperty);
                }

                if (args.Effects != DragDropEffects.None)
                {
                    _dropPossible = true;
                }

                if (tvi != null)
                {
                    _currentItem = tvi;
                    // Tell that item to re-calculate the IsPossibleDropTarget property
                    _currentItem.InvalidateProperty(IsPossibleDropTargetProperty);

                    // if the item isn't expanded
                    if (!tvi.IsExpanded)
                    {
                        // check if the item has changed
                        if (_lastUnexpanded != tvi)
                        {
                            // take note of when it was first made drop target
                            _lastUnexpanded = tvi;
                            _lastUnexpandedUpdated = DateTime.Now;
                        }
                        else
                        {
                            // if the same unexpanded item has been drop target for more than a second
                            // expand it
                            if (DateTime.Now.Subtract(_lastUnexpandedUpdated).TotalMilliseconds > 2000)
                            {
                                tvi.IsExpanded = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when the drag cursor leaves the TreeViewItem
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        static void OnDragLeave(object sender, DragEventArgs args)
        {
            lock (IsPossibleDropTargetProperty)
            {
                _dropPossible = false;

                if (_currentItem != null)
                {
                    // Tell the item that previously had the mouse that it no longer does.
                    DependencyObject oldItem = _currentItem;
                    _currentItem = null;
                    oldItem.InvalidateProperty(IsPossibleDropTargetProperty);
                }

                TreeViewItem? tvi = sender as TreeViewItem;
                if (tvi != null)
                {
                    _currentItem = tvi;
                    tvi.InvalidateProperty(IsPossibleDropTargetProperty);
                }
            }
        }
        #endregion
    }
}
