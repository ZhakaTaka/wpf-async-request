using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VerySimpleDemo
{
    public class AsyncCommand : ICommand
    {
        public AsyncCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return !IsExecutionPreveted && (_canExecute == null || _canExecute());
        }

        public void Execute(object parameter)
        {
            IsExecutionPreveted = true;
            _execute().ContinueWith(t => { IsExecutionPreveted = false; }, TaskContinuationOptions.ExecuteSynchronously);
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region IsExecutionPreveted

        private bool IsExecutionPreveted
        {
            get
            {
                return _isExecutionPreveted;
            }

            set
            {
                _isExecutionPreveted = value;
                OnCanExecuteChanged();
            }
        }

        private bool _isExecutionPreveted;

        #endregion

        private readonly Func<Task> _execute;

        private readonly Func<bool> _canExecute;
    }
}
