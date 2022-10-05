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
        /// Adds the specified undo action.
        /// </summary>
        /// <param name="action">Action to add</param>
        void Add(UndoActionModel action);

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

        /// <summary>
        /// Performs the specified action immediately and registers an undo action for it.
        /// </summary>
        /// <param name="name">Name of the action to run</param>
        /// <param name="action">Action to run immediately and if the action is redone</param>
        /// <param name="undoAction">Action to run when undoing the action</param>
        void Do(string name, Action action, Action undoAction);

        /// <summary>
        /// Starts a new aggregate action and returns an object that should be disposed when the actions in the scope have been committed.
        /// </summary>
        /// <param name="name">Name of the aggregate action to set</param>
        /// <param name="source">Source object for the action</param>
        /// <param name="operation">Identifier determining what the action does</param>
        /// <param name="afterRedo">Action to run after redoing the aggregate action</param>
        /// <param name="afterUndo">Action to run after undoing the aggregate action</param>
        /// <returns></returns>
        UndoAggregateScope StartAggregateScope(string name, object? source = null, string? operation = null, Action? afterUndo = null, Action? afterRedo = null);

        /// <summary>
        /// Attempts to undo a specific action without undoing actions performed afterwards. This should be used with caution, as
        /// this can potentially leave the undo action queue in a way that doesn't make sense.
        /// After being undone the action is removed from the queue and cannot be redone.
        /// Returns a value indicating if the action was found in the queue.
        /// </summary>
        /// <param name="source">Source object for the action</param>
        /// <param name="operation">Identifier determining what the action does</param>
        /// <returns>Value indicating if the action was found, undone and removed.</returns>
        bool UndoAndRemoveSpecific(object source, string? operation);
    }
}