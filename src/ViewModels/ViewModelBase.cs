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
    public abstract class ViewModelBase : BindableBase, IServiceProvider
    {
        private bool _isInitialized;
        private object? _parameter;
        private object? _parentViewModel;
        private readonly Lazy<IServiceContainer> _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        protected ViewModelBase()
        {
           _services = new Lazy<IServiceContainer>(() => new ServiceProvider(this));
        }

        #region Properties

#if NETFRAMEWORK || WINDOWS
        /// <summary>
        /// Gets the dispatcher associated with the UI thread.
        /// </summary>
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
        /// Gets or sets a parameter associated with the ViewModel.
        /// </summary>
        public object? Parameter
        {
            get => _parameter;
            set
            {
                if (_parameter == value) return;
                _parameter = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the parent ViewModel.
        /// Throws an <see cref="InvalidOperationException"/> if set to itself.
        /// </summary>
        public object? ParentViewModel
        {
            get => _parentViewModel;
            set
            {
                if (_parentViewModel == value) return;
                if (value == this) throw new InvalidOperationException("ParentViewModel cannot be set to itself.");
                _parentViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the service container for dependency injection and service resolution.
        /// </summary>
        public IServiceContainer Services => _services.Value;

        /// <summary>
        /// Gets the thread on which the current instance was created.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Thread Thread
#if NETFRAMEWORK || WINDOWS
            => Dispatcher.Thread;
#else
        { get; } = Thread.CurrentThread;
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
        /// Gets a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>An instance of the requested service, or <c>null</c> if the service is not available.</returns>
        public T? GetService<T>()
        {
            return (T?)((IServiceProvider)this).GetService(typeof(T));
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type <paramref name="serviceType"/>. 
        /// If there is no service object of type <paramref name="serviceType"/>, returns <c>null</c>.</returns>
        object? IServiceProvider.GetService(Type serviceType)
        {
            var service = Services.GetService(serviceType);
            if (service is null && ParentViewModel is IServiceProvider serviceProvider)
            {
                service = serviceProvider.GetService(serviceType);
            }
            return service ?? ServiceProvider.Default.GetService(serviceType);
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
        protected virtual Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// When overridden in a derived class, asynchronously performs the uninitialization logic for the ViewModel.
        /// This method should contain any custom uninitialization logic required by the derived ViewModel class.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the uninitialization process.</param>
        /// <returns>A task that represents the asynchronous uninitialization operation.</returns>
        protected virtual Task OnUninitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

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
