using System;
using System.Windows.Input;

namespace Youtube_Music.Commands
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _action;
        private bool _canExecuteEnabled;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action action, bool canExecute = true)
        {
            _action = action;
            _canExecuteEnabled = canExecute;
        }

        public bool CanExecute(object parameter) => CanExecuteEnabled;

        public void Execute(object parameter) => _action?.Invoke();

        public void Execute() => _action?.Invoke();

        private void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public bool CanExecuteEnabled
        {
            get => _canExecuteEnabled;
            set
            {
                _canExecuteEnabled = value;
                OnCanExecuteChanged();
            }
        }
    }
}