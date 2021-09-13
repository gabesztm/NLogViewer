using System;
using System.Windows.Input;

namespace NLogViewer
{
    internal class SimpleCommand : ICommand
    {
        Action _methodToExecute;
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _methodToExecute?.Invoke();
        }

        public SimpleCommand(Action methodToExecute)
        {
            _methodToExecute = methodToExecute;
        }
    }
}
