using System;

namespace OpenSAE.Models
{
    public interface IUndoModel
    {
        /// <summary>
        /// Sets an undo action. The action will be added to the queue unless the latest action in queue has the same source and identifier.
        /// If this is the case, it will be updated instead.
        /// </summary>
        /// <param name="name">Name of the action to set</param>
        /// <param name="source">Source object for the action</param>
        /// <param name="operation">Identifier determining what the action does</param>
        /// <param name="undo">Function to run when undoing the action</param>
        /// <param name="redo">Function to run when redoing the action</param>
        void Set(string name, object source, string operation, Action undo, Action redo);

        /// <summary>
        /// Adds an undo action. Will always be added to the queue. Use <see cref="Set"/> to be able to update latest action.
        /// </summary>
        /// <param name="name">Name of the action to set</param>
        /// <param name="undo">Function to run when undoing the action</param>
        /// <param name="redo">Function to run when redoing the action</param>
        void Add(string name, Action undo, Action redo);

        /// <summary>
        /// Begins an aggregate action in the undo queue. All actions performed until <see cref="EndAggregate"/> is called
        /// will be grouped into a single action that can undo/redo them all in one step.
        /// </summary>
        /// <param name="name">Name of the aggregate action to set</param>
        /// <param name="source">Source object for the action</param>
        /// <param name="operation">Identifier determining what the action does</param>
        /// <param name="afterRedo">Action to run after redoing the aggregate action</param>
        /// <param name="afterUndo">Action to run after undoing the aggregate action</param>
        void BeginAggregate(string name, object? source, string? operation, Action? afterUndo = null, Action? afterRedo = null);

        /// <summary>
        /// Clears all actions from the queue
        /// </summary>
        void Clear();

        /// <summary>
        /// Ends the last aggregate action started with <see cref="BeginAggregate"/>
        /// </summary>
        void EndAggregate();

        /// <summary>
        /// Clears all actions from the queue and adds an empty action that represents the base state.
        /// </summary>
        /// <param name="name">Name to use for the empty base action.</param>
        void ResetWith(string name);
    }
}