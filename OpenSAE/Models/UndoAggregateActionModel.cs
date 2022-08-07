using System;
using System.Collections.Generic;

namespace OpenSAE.Models
{
    public class UndoAggregateActionModel : UndoActionModel
    {
        public List<UndoActionModel> Actions { get; }
            = new();

        public UndoAggregateActionModel(string name, object? source, string? operation, Action? afterUndo, Action? afterRedo, bool isPerformed) 
            : base(name, source, operation, null, null, isPerformed)
        {
            AfterUndoAction = afterUndo;
            AfterRedoAction = afterRedo;
        }

        public override Action? RedoAction => _redoAction ?? AggregateRedo;

        public override Action? UndoAction => _undoAction ?? AggregateUndo;

        public Action? AfterUndoAction { get; }

        public Action? AfterRedoAction { get; }

        private void AggregateUndo()
        {
            for (int i = Actions.Count - 1; i >= 0; i--)
            {
                Actions[i].UndoAction?.Invoke();
            }

            AfterUndoAction?.Invoke();
        }

        private void AggregateRedo()
        {
            foreach (var action in Actions)
            {
                action.RedoAction?.Invoke();
            }

            AfterRedoAction?.Invoke();
        }
    }
}
