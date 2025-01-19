using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Interface for an asynchronous command, extending the functionality of IRelayCommand.
    /// </summary>
    public interface IAsyncCommand : IRelayCommand
    {
        /// <summary>
        /// Gets the current cancellation token source.
        /// This is useful for tracking and managing ongoing operations,
        /// and it is relevant when querying from within the execute method.
        /// </summary>
        CancellationTokenSource? CancellationTokenSource { get; }

        /// <summary>
        /// Occurs when an exception is thrown during the execution of the command.
        /// </summary>
        event EventHandler<ErrorEventArgs>? ExecutionFailed;

        /// <summary>
        /// Gets whether cancellation has been requested for this command.
        /// </summary>
        bool IsCancellationRequested { get; }

        /// <summary>
        /// Cancels the current asynchronous operations.
        /// If a command is executing, this method triggers cancellation through the associated cancellation token.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter to be used by the command.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(object? parameter);

        /// <summary>
        /// Executes the command asynchronously with cancellation support.
        /// </summary>
        /// <param name="parameter">The parameter to be used by the command.</param>
        /// <param name="cancellationToken">A cancellation token for managing the command execution.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(object? parameter, CancellationToken cancellationToken);

        /// <summary>
        /// Resets the cancellation state back to not canceled if it was in the notifying state.
        /// </summary>
        void ResetCancel();
    }
}
