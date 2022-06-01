using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TrumpX.Toolkit.Wpf.Mvvm
{
    public sealed class DelegateCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        private readonly bool _autoExecCanExecuteChanged;
        private EventHandler _canExecuteChanged;

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute = default, bool autoExecCanExecuteChanged = false)
        {
            if (canExecute == null)
            {
                autoExecCanExecuteChanged = false;
            }
            _execute = execute;
            _canExecute = canExecute;
            _autoExecCanExecuteChanged = autoExecCanExecuteChanged;
        }

        public event EventHandler CanExecuteChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                if (_autoExecCanExecuteChanged)
                {
                    CommandManager.RequerySuggested += value;
                }
                _canExecuteChanged += value;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                if (_autoExecCanExecuteChanged)
                {
                    CommandManager.RequerySuggested -= value;
                }
                _canExecuteChanged -= value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _execute?.Invoke(parameter);
            }
        }

        public void NotifyCanExecuteChanged()
        {
            _canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}