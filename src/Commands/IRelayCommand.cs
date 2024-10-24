using System.Windows.Input;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Defines a command that can notify changes in its ability to execute.
    /// </summary>
    public interface IRelayCommand : ICommand
    {
        /// <summary>
        /// Gets or sets a value indicating whether concurrent execution of the command is allowed.
        /// </summary>
        bool AllowConcurrentExecution { get; set; }

        /// <summary>
        /// Gets a value indicating whether the command is currently executing.
        /// </summary>
        bool IsExecuting { get; }

        /// <summary>
        /// Raises the <see cref="ICommand.CanExecuteChanged"/> event.
        /// </summary>
        void RaiseCanExecuteChanged();
    }
}
