using System;

namespace OpenSAE.Models
{
    public class UndoAggregateScope : IDisposable
    {
        private readonly IUndoModel _undoModel;

        public UndoAggregateScope(IUndoModel undoModel)
        {
            _undoModel = undoModel;
        }

        public void Dispose()
        {
            _undoModel.EndAggregate();
            GC.SuppressFinalize(this);
        }
    }
}
