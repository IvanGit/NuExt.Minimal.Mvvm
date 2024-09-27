using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
#if NETFRAMEWORK || WINDOWS
using System.Windows.Threading;
#endif

namespace Minimal.Mvvm
{
    /// <summary>
    /// Base class for ViewModels.
    /// </summary>
    public abstract class ViewModelBase : BindableBase
    {
        private bool _isInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        protected ViewModelBase()
        {
#if !NETFRAMEWORK && !WINDOWS
            Thread = Thread.CurrentThread;
#endif
        }

        #region Properties

#if NETFRAMEWORK || WINDOWS
        public Dispatcher Dispatcher { get; } = Dispatcher.CurrentDispatcher;
#endif

        /// <summary>
        /// Gets a value indicating whether the ViewModel has been initialized.
        /// </summary>
        public bool IsInitialized
        {
            get => _isInitialized;
            private set => SetProperty(ref _isInitialized, value);
        }

        /// <summary>
        /// Gets the thread on which the current instance was created.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Thread Thread
#if NETFRAMEWORK || WINDOWS
            => Dispatcher.Thread;
#else
        { get; }
#endif

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the current thread is the same as the thread on which this instance was created.
        /// </summary>
        /// <returns>True if the current thread is the same as the creation thread; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAccess()
        {
#if NETFRAMEWORK || WINDOWS
            return Dispatcher.CheckAccess();
#else
            return Thread == Thread.CurrentThread;
#endif
        }

        /// <summary>
        /// Asynchronously initializes the ViewModel.
        /// This method performs various checks and throws exceptions if certain conditions are met,
        /// such as if the parent ViewModel is null or if the ViewModel is already initialized.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token to cancel the initialization process.</param>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (IsInitialized)
            {
                return;
            }
            try
            {
                await OnInitializeAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.Assert(ex is OperationCanceledException, ex.Message);
                throw;
            }
            IsInitialized = true;
        }

        /// <summary>
        /// When overridden in a derived class, asynchronously performs the initialization logic for the ViewModel.
        /// This method should contain any custom initialization logic required by the derived ViewModel class.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the initialization process.</param>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        protected abstract Task OnInitializeAsync(CancellationToken cancellationToken);

        /// <summary>
        /// When overridden in a derived class, asynchronously performs the uninitialization logic for the ViewModel.
        /// This method should contain any custom uninitialization logic required by the derived ViewModel class.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the uninitialization process.</param>
        /// <returns>A task that represents the asynchronous uninitialization operation.</returns>
        protected abstract Task OnUninitializeAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously uninitializes the ViewModel.
        /// This method performs various checks and throws exceptions if certain conditions are met,
        /// such as if the ViewModel is already uninitialized.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token to cancel the uninitialization process.</param>
        /// <returns>A task that represents the asynchronous uninitialization operation.</returns>
        public async Task UninitializeAsync(CancellationToken cancellationToken = default)
        {
            if (!IsInitialized)
            {
                return;
            }
            try
            {
                await OnUninitializeAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                throw;
            }
            IsInitialized = false;
        }

        #endregion
    }
}
