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
        private object? _source;
        private string? _operation;
        private string _name;
        protected Action? _undoAction;
        protected Action? _redoAction;

        public UndoActionModel(string name, object? source, string? operation, Action? undo, Action? redo, bool isPerformed)
        {
            _name = name;
            _isPerformed = isPerformed;
            _source = source;
            _operation = operation;
            _undoAction = undo;
            _redoAction = redo;
        }

        public object? Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        public string? Operation
        {
            get => _operation;
            set => SetProperty(ref _operation, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public virtual Action? UndoAction => _undoAction;

        public virtual Action? RedoAction => _redoAction;

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

        /// <summary>
        /// Updates all properties from the specified model except the undo action. This can be used when consecutive actions
        /// modify the same source with the same operation, to only have one undo action.
        /// </summary>
        /// <param name="newModel"></param>
        public void Update(UndoActionModel newModel)
        {
            Name = newModel.Name;
            Source = newModel.Source;
            Operation = newModel.Operation;
            IsPerformed = newModel.IsPerformed;
            _redoAction = newModel.RedoAction;
        }
    }
}
