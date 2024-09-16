using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Provides a base implementation for commands.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public abstract class CommandBase<T>: ICommand<T>, IRelayCommand, INotifyPropertyChanged
    {
        private readonly Func<T?, bool>? _canExecute;
        protected internal volatile int ExecutingCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase{T}"/> class.
        /// </summary>
        /// <param name="canExecute">The function to determine if the command can execute.</param>
        protected CommandBase(Func<T?, bool>? canExecute)
        {
            _canExecute = canExecute;
        }

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the command is currently executing.
        /// </summary>
        public bool IsExecuting => ExecutingCount != 0;

        #endregion

        #region Events

        private event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        event EventHandler? ICommand.CanExecuteChanged
        {
            add => CanExecuteChanged += value;
            remove => CanExecuteChanged -= value;
        }

        private event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        [DebuggerStepThrough]
        public bool CanExecute(T? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// Supports multiple execution.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public abstract void Execute(T? parameter);

        bool ICommand.CanExecute(object? parameter)
        {
            return CanExecute((T?)parameter);
        }
        void ICommand.Execute(object? parameter)
        {
            Execute((T?)parameter);
        }

        protected virtual bool OnExecuted()
        {
            if (Interlocked.Decrement(ref ExecutingCount) != 0) return false;
            OnPropertyChanged(EventArgsCache.IsExecutingPropertyChanged);
            RaiseCanExecuteChanged();
            return true;
        }

        protected virtual bool OnExecuting()
        {
            if (Interlocked.Increment(ref ExecutingCount) != 1) return false;
            OnPropertyChanged(EventArgsCache.IsExecutingPropertyChanged);
            RaiseCanExecuteChanged();
            return true;
        }

        /// <summary>
        /// Notifies listeners about changes in property values.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the CanExecuteChanged event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }

    internal static class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs IsExecutingPropertyChanged = new(nameof(IRelayCommand.IsExecuting));
    }
}
