using System;

namespace OpenSAE.Models
{
    public class DummyUndoModel : IUndoModel
    {
        public void Add(string name, Action undo, Action redo)
        {
        }

        public void Add(UndoActionModel action)
        {
        }

        public void BeginAggregate(string name, object? source, string? operation, Action? afterUndo = null, Action? afterRedo = null)
        {
        }

        public void Clear()
        {
        }

        public void Do(string name, Action action, Action undoAction)
        {
            action?.Invoke();
        }

        public void EndAggregate()
        {
        }

        public void ResetWith(string name)
        {
        }

        public void Set(string name, object source, string operation, Action undo, Action redo)
        {
        }

        public UndoAggregateScope StartAggregateScope(string name, object? source = null, string? operation = null, Action? afterUndo = null, Action? afterRedo = null)
        {
            return new UndoAggregateScope(this);
        }
    }
}
