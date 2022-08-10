using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenSAE
{
    internal class SimpleCommand : ICommand
    {
        public event EventHandler<object>? Executed;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Executed?.Invoke(this, parameter);
        }

        public event EventHandler? CanExecuteChanged;
    }
}
