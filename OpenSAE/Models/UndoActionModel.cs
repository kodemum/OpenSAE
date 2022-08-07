using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace OpenSAE.Models
{
    /// <summary>
    /// Represents an action that can be undone or redone
    /// </summary>
    public class UndoActionModel : ObservableObject
    {
        private bool _isPerformed;

        public UndoActionModel(string name, Action? undo, Action? redo, bool isPerformed)
        {
            _isPerformed = isPerformed;
            Name = name;
            UndoAction = undo;
            RedoAction = redo;
        }

        public string Name { get; }

        public virtual Action? UndoAction { get; }

        public virtual Action? RedoAction { get; }

        public bool IsPerformed
        {
            get => _isPerformed;
            set => SetProperty(ref _isPerformed, value);
        }

        public void PerformRedo()
        {
            if (!IsPerformed)
            {
                RedoAction?.Invoke();
                IsPerformed = true;
            }
        }

        public void PerformUndo()
        {
            if (IsPerformed)
            {
                UndoAction?.Invoke();
                IsPerformed = false;
            }
        }
    }
}
