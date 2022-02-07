using System;
using System.Windows.Input;

namespace SortingStantion.Utilites
{
    class DelegateCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parametr)
        {
            return this.canExecute == null || this.canExecute(parametr);
        }

        public void Execute(object parametr)
        {
            this.execute(parametr);
        }
    }
}
