using System;

namespace OpenSAE.Models
{
    public interface IUndoModel
    {
        void Add(string name, Action undo, Action redo);
        void BeginAggregate(string name);
        void Clear();
        void EndAggregate();
        void ResetWith(string name);
    }
}