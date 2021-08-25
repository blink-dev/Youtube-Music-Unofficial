using System;
using System.Windows.Input;

namespace Youtube_Music.Commands
{
    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _action;
        private bool _canExecuteEnabled;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> action, bool canExecute = true)
        {
            _action = action;
            _canExecuteEnabled = canExecute;
        }

        public bool CanExecute(object parameter) => CanExecuteEnabled;

        public void Execute(object parameter)
        {
            if (parameter is T t)
            {
                _action?.Invoke(t);
            }
            else if (parameter == null && !typeof(T).IsValueType)
            {
                _action?.Invoke(default);
            }
            else
            {
                throw new InvalidCastException($"parameter must be a {typeof(T)}");
            }
        }

        public void Execute() => _action?.Invoke(default);

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
