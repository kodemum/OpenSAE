using System;

namespace OpenSAE.Models
{
    public class UndoAggregateScope : IDisposable
    {
        private readonly UndoModel _undoModel;

        public UndoAggregateScope(UndoModel undoModel)
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
