using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Represents an asynchronous command that can execute with a parameter of type T.
    /// Supports multiple execution.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public class AsyncCommand<T> : CommandBase<T>, IAsyncCommand<T>
    {
        private readonly Func<T?, Task> _execute;
        private readonly AsyncLocal<CancellationTokenSource?> _cancellationTokenSource = new();
        internal readonly ConcurrentDictionary<CancellationTokenSource, int> ExecutingTasks = new();

        private volatile int _state;
        private const int NotCanceledState = 0;
        private const int NotifyingState = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The asynchronous execution logic.</param>
        /// <param name="canExecute">The logic to determine if the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown if the execute argument is null.</exception>
        public AsyncCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute):base(canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommand{T}"/> class with no canExecute logic.
        /// </summary>
        /// <param name="execute">The asynchronous execution logic.</param>
        public AsyncCommand(Func<T?, Task> execute) : this(execute, null)
        {

        }

        #region Properties

        /// <summary>
        /// Gets or sets the current cancellation token source. This is useful for tracking and managing ongoing operations,
        /// and it is relevant when querying from within the execute method.
        /// </summary>
        public CancellationTokenSource? CancellationTokenSource
        {
            get => _cancellationTokenSource.Value;
            private set => _cancellationTokenSource.Value = value;
        }

        /// <summary>
        /// Gets a value indicating whether the current execution context should be captured 
        /// and used to continue asynchronous operations in the ExecuteAsync method.
        /// </summary>
        public bool ContinueOnCapturedContext
        {
            get;
#if NET
            init;
#else
            set;
#endif
        }

        /// <summary>Gets whether cancellation has been requested for this <see cref="AsyncCommand{T}" />.</summary>
        public bool IsCancellationRequested => _state != NotCanceledState;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an exception is thrown during the execution of the command.
        /// </summary>
        public event EventHandler<ErrorEventArgs>? ExecutionFailed;

        #endregion

        #region Methods

        /// <summary>
        /// Cancels the current asynchronous operations.
        /// </summary>
        public void Cancel()
        {
            if (Interlocked.CompareExchange(ref _state, NotifyingState, NotCanceledState) != NotCanceledState)
            {
                return;
            }
            foreach (var pair in ExecutingTasks)
            {
                var cts = pair.Key;
                try
                {
                    cts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    //do nothing
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                    throw;
                }
            }
        }

        /// <summary>
        /// Executes the command synchronously and asynchronously.
        /// This method triggers the ExecutionFailed event if an exception occurs.
        /// Supports multiple execution.
        /// </summary>
        /// <param name="parameter">The parameter to be used by the command.</param>
        public override async void Execute(T? parameter)
        {
            try
            {
                await ExecuteAsync(parameter);
            }
            catch (OperationCanceledException)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// This method triggers the ExecutionFailed event if an exception occurs.
        /// Supports multiple execution.
        /// </summary>
        /// <param name="parameter">The parameter to be used by the command.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task ExecuteAsync(T? parameter)
        {
            return ExecuteAsync(parameter, default);
        }

        /// <summary>
        /// Executes the command asynchronously with a cancellation token.
        /// This method triggers the ExecutionFailed event if an exception occurs.
        /// Supports multiple execution.
        /// </summary>
        /// <param name="parameter">The parameter to be used by the command.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync(T? parameter, CancellationToken cancellationToken)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            using var cts = cancellationToken.CanBeCanceled ? 
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : 
                new CancellationTokenSource();
            if (IsCancellationRequested)
            {
#if NET8_0_OR_GREATER
                await cts.CancelAsync();
#else
                cts.Cancel();
#endif
            }
            cts.Token.ThrowIfCancellationRequested();
            CancellationTokenSource = cts;
            try
            {
                OnExecuting();
                if (IsCancellationRequested)
                {
#if NET8_0_OR_GREATER
                    await cts.CancelAsync();
#else
                    cts.Cancel();
#endif
                }
                cts.Token.ThrowIfCancellationRequested();
                await _execute(parameter).ConfigureAwait(ContinueOnCapturedContext);
            }
            catch (OperationCanceledException ex)
            {
                Debug.Assert(cts.IsCancellationRequested, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                OnExecutionFailed(ex);
                throw;
            }
            finally
            {
                OnExecuted();
            }
        }

        /// <summary>
        /// Executes the command asynchronously with an object parameter (used for non-generic execution).
        /// </summary>
        /// <param name="parameter">The parameter to be used by the command.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task IAsyncCommand.ExecuteAsync(object? parameter)
        {
            return ExecuteAsync((T?)parameter);
        }

        /// <summary>
        /// Executes the command asynchronously with an object parameter and cancellation support (used for non-generic execution).
        /// </summary>
        /// <param name="parameter">The parameter to be used by the command.</param>
        /// <param name="cancellationToken">A cancellation token for managing the command execution.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task IAsyncCommand.ExecuteAsync(object? parameter, CancellationToken cancellationToken)
        {
            return ExecuteAsync((T?)parameter, cancellationToken);
        }

        protected override bool OnExecuted()
        {
            Debug.Assert(CancellationTokenSource != null, $"{nameof(CancellationTokenSource)} is null");
            bool result = ExecutingTasks.TryRemove(CancellationTokenSource, out _);
            Debug.Assert(result);
            CancellationTokenSource = null;
            return base.OnExecuted();
        }

        protected override bool OnExecuting()
        {
            Debug.Assert(CancellationTokenSource != null, $"{nameof(CancellationTokenSource)} is null");
            bool result = ExecutingTasks.TryAdd(CancellationTokenSource, Environment.CurrentManagedThreadId);
            Debug.Assert(result);
            return base.OnExecuting();
        }

        /// <summary>
        /// Raises the ExecutionFailed event.
        /// </summary>
        /// <param name="exception">The exception that occurred during execution.</param>
        private void OnExecutionFailed(Exception exception)
        {
            ExecutionFailed?.Invoke(this, new ErrorEventArgs(exception));
        }

        /// <summary>
        ///  Resets the cancellation state back to not canceled if it was in the notifying state.
        /// </summary>
        public void ResetCancel()
        {
            Interlocked.CompareExchange(ref _state, NotCanceledState, NotifyingState);
        }

        #endregion
    }
}
