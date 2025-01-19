using System;

namespace Minimal.Mvvm
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other objects by invoking delegates.
    /// The default return value for the CanExecute method is 'true'.
    /// This implementation allows you to pass a parameter of type T to the Execute and CanExecute methods.
    /// Supports multiple execution.
    /// </summary>
    /// <typeparam name="T">The type of the parameter passed to the command.</typeparam>
    public class RelayCommand<T> : CommandBase<T>
    {
        private readonly Action<T> _execute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic. If null, the command can always execute.</param>
        /// <exception cref="ArgumentNullException">Thrown if the execute argument is null.</exception>
        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute) : base(canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <exception cref="ArgumentNullException">Thrown if the execute argument is null.</exception>
        public RelayCommand(Action<T> execute) : this(execute, null)
        {

        }

        #region Methods

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// Supports multiple execution.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public override void Execute(T parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }
            try
            {
                OnExecuting();
                _execute(parameter);
            }
            finally
            {
                OnExecuted();
            }
        }

        #endregion
    }
}
