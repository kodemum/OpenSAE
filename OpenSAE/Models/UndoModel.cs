using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Models
{
    public class UndoModel : ObservableObject, IUndoModel
    {
        private UndoActionModel? _currentAction;

        public ObservableCollection<UndoActionModel> UndoActions { get; }
            = new ObservableCollection<UndoActionModel>();

        public RelayCommand UndoCommand { get; }

        public RelayCommand RedoCommand { get; }

        public UndoActionModel? CurrentAction
        {
            get => _currentAction;
            set
            {
                int currentIndex = CurrentActionIndex;

                if (SetProperty(ref _currentAction, value) && value != null)
                {
                    int index = UndoActions.IndexOf(value);

                    if (index != -1)
                    {
                        if (index < currentIndex)
                        {
                            // older action selected, undo every action back to the selected
                            for (int i = currentIndex; i > index; i--)
                            {
                                UndoActions[i].PerformUndo();
                            }
                        }
                        else if (currentIndex != -1)
                        {
                            // newer action selected, redo every action up to and including the selected
                            for (int i = currentIndex; i <= index; i++)
                            {
                                UndoActions[i].PerformRedo();
                            }
                        }
                    }

                    UndoCommand.NotifyCanExecuteChanged();
                    RedoCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public int CurrentActionIndex
        {
            get => _currentAction == null ? -1 : UndoActions.IndexOf(_currentAction);
            set => CurrentAction = value == -1 ? null : UndoActions[value];
        }

        private readonly Stack<UndoAggregateActionModel> _aggregateStack = new();

        public UndoModel()
        {
            UndoCommand = new RelayCommand(() => CurrentActionIndex--, () => CurrentActionIndex > 0);
            RedoCommand = new RelayCommand(() => CurrentActionIndex++, () => CurrentActionIndex < UndoActions.Count - 1);
        }

        public void Clear()
        {
            UndoActions.Clear();
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
        }

        public void Add(string name, Action undo, Action redo)
        {
            Set(name, null, null, undo, redo);
        }

        public void Do(string name, Action action, Action undoAction)
        {
            action.Invoke();

            Add(name, undoAction, action);
        }

        public void Set(string name, object? source, string? operation, Action undo, Action redo)
        {
            UndoActionModel newAction = new(name, source, operation, undo, redo, true);

            if (_aggregateStack.TryPeek(out var currentAggregate))
            {
                // if an aggregate action is in progress, add the action to it
                currentAggregate.Actions.Add(newAction);
            }
            else
            {
                CommitAction(newAction);
            }
        }

        private void CommitAction(UndoActionModel newAction)
        {
            // if adding a new action remove any after the currently selected action that haven't been performed
            if (CurrentAction != null)
            {
                int currentIndex = UndoActions.IndexOf(CurrentAction);

                for (int i = UndoActions.Count - 1; i > currentIndex; i--)
                {
                    UndoActions.RemoveAt(i);
                }
            }

            // check if the last action is for the same source and operation
            var currentActiveAction = CurrentAction ?? UndoActions.LastOrDefault();
            if (currentActiveAction != null)
            {
                if (currentActiveAction.Source != null && currentActiveAction.Source == newAction.Source && currentActiveAction.Operation == newAction.Operation)
                {
                    // in this case we'll overwrite everything but the undo action
                    currentActiveAction.Update(newAction);
                    return;
                }
            }

            UndoActions.Add(newAction);
            _currentAction = newAction;
            OnPropertyChanged(nameof(CurrentAction));

            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
        }

        public void BeginAggregate(string name, object? source, string? operation, Action? afterUndo, Action? afterRedo)
        {
            UndoAggregateActionModel newAggregate = new(name, source, operation, afterUndo, afterRedo, true);

            if (_aggregateStack.TryPeek(out var existingAggregate))
            {
                existingAggregate.Actions.Add(newAggregate);
            }

            _aggregateStack.Push(newAggregate);
        }

        public void EndAggregate()
        {
            var aggregate = _aggregateStack.Pop();

            if (_aggregateStack.Count == 0)
            {
                // last aggregate reached - commit it
                CommitAction(aggregate);
            }
        }

        public void ResetWith(string name)
        {
            Clear();
            UndoActions.Add(new UndoActionModel(name, null, null, null, null, true));
        }
    }
}
