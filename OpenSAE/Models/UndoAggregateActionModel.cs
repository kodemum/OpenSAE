using System;
using System.Collections.Generic;

namespace OpenSAE.Models
{
    public class UndoAggregateActionModel : UndoActionModel
    {
        public List<UndoActionModel> Actions { get; }
            = new();

        public UndoAggregateActionModel(string name, bool isPerformed) 
            : base(name, null, null, isPerformed)
        {
        }

        public override Action? RedoAction => AggregateRedo;

        public override Action? UndoAction => AggregateUndo;

        private void AggregateUndo()
        {
            for (int i = Actions.Count - 1; i >= 0; i--)
            {
                Actions[i].UndoAction?.Invoke();
            }
        }

        private void AggregateRedo()
        {
            foreach (var action in Actions)
            {
                action.RedoAction?.Invoke();
            }
        }
    }
}
