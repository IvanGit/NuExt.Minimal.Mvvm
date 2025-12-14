using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#if NETFRAMEWORK || WINDOWS
using System.Windows;
using System.Windows.Threading;
#endif

namespace Minimal.Mvvm
{
    /// <summary>
    /// Base class for ViewModels.
    /// </summary>
    public abstract class ViewModelBase : BindableBase, IServiceProvider
    {
        private readonly Lazy<IServiceContainer> _services;
        private readonly SemaphoreSlim _initLock = new(1, 1);

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

        private static bool? s_isInDesignMode;
        /// <summary>
        /// Gets a value indicating whether the ViewModel is in design mode.
        /// </summary>
        public static bool IsInDesignMode
        {
            get
            {
                s_isInDesignMode ??= (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;
                return s_isInDesignMode.Value;
            }
        }
#endif

        private volatile bool _isInitialized;
        /// <summary>
        /// Gets a value indicating whether the ViewModel has been initialized.
        /// This property is thread-safe for reading.
        /// </summary>
        public bool IsInitialized => _isInitialized;

        private object? _parameter;
        /// <summary>
        /// Gets or sets a parameter associated with the ViewModel.
        /// </summary>
        public object? Parameter
        {
            get => _parameter;
            set
            {
                if (_parameter == value) return;
                if (!CanSetProperty(_parameter, value)) return;
                _parameter = value;
                OnPropertyChanged(EventArgsCache.ParameterPropertyChanged);
            }
        }

        private object? _parentViewModel;
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
                if (value == this) ThrowInvalidParentViewModelAssignment();
                if (!CanSetProperty(_parentViewModel, value)) return;
                _parentViewModel = value;
                OnPropertyChanged(EventArgsCache.ParentViewModelPropertyChanged);
            }
        }

        /// <summary>
        /// Gets the service container for dependency injection and service resolution.
        /// </summary>
        public IServiceContainer Services => _services.Value;

        /// <summary>
        /// Gets the thread on which the current instance was created.
        /// </summary>
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
            return (T?)GetService(typeof(T), null);
        }

        /// <summary>
        /// Gets the named service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <param name="name">
        /// The name of the service to resolve. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <returns>An instance of the requested service, or <c>null</c> if the service is not available.</returns>
        public T? GetService<T>(string name)
        {
            return (T?)GetService(typeof(T), name);
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type <paramref name="serviceType"/>. 
        /// If there is no service object of type <paramref name="serviceType"/>, returns <c>null</c>.</returns>
        object? IServiceProvider.GetService(Type serviceType)
        {
            return GetService(serviceType, null);
        }

        /// <summary>
        /// Gets the named service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <param name="name">
        /// The name of the service to resolve. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <returns>A service object of type <paramref name="serviceType"/>. 
        /// If there is no service object of type <paramref name="serviceType"/>, returns <c>null</c>.</returns>
        public object? GetService(Type serviceType, string? name)
        {
            var service = Services.GetService(serviceType, name);
            switch (service)
            {
                case null when ParentViewModel is ViewModelBase parentViewModel:
                    service = parentViewModel.GetService(serviceType, name);
                    break;
                case null when ParentViewModel is IServiceProvider serviceProvider:
                    service = serviceProvider.GetService(serviceType);
                    break;
            }
            return service ?? ServiceProvider.Default.GetService(serviceType, name);
        }

        /// <summary>
        /// Asynchronously initializes the ViewModel. This method is thread-safe and idempotent.
        /// </summary>
        /// <param name="cancellationToken">An optional cancellation token to cancel the initialization process.</param>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        /// <remarks>
        /// If the ViewModel is already initialized, the method returns immediately.
        /// Otherwise, it acquires an initialization lock, calls <see cref="OnInitializeAsync"/>,
        /// updates the initialized state, and notifies property change after releasing the lock.
        /// </remarks>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_isInitialized) return;

            await _initLock.WaitAsync(cancellationToken);
            try
            {
                if (_isInitialized) return;

                await OnInitializeAsync(cancellationToken);

                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }

            OnPropertyChanged(EventArgsCache.IsInitializedPropertyChanged);
        }

        /// <summary>
        /// Asynchronously uninitializes the ViewModel if it was initialized. This method is thread-safe.
        /// </summary>
        /// <param name="cancellationToken">An optional cancellation token to cancel the uninitialization process.</param>
        /// <returns>A task that represents the asynchronous uninitialization operation.</returns>
        /// <remarks>
        /// If the ViewModel is not initialized, the method returns immediately.
        /// Otherwise, it acquires the initialization lock, calls <see cref="OnUninitializeAsync"/>,
        /// updates the initialized state, and notifies property change after releasing the lock.
        /// </remarks>
        public async Task UninitializeAsync(CancellationToken cancellationToken = default)
        {
            if (!_isInitialized) return;

            await _initLock.WaitAsync(cancellationToken);
            try
            {
                if (!_isInitialized) return;

                await OnUninitializeAsync(cancellationToken);
                _isInitialized = false;
            }
            finally
            {
                _initLock.Release();
            }

            OnPropertyChanged(EventArgsCache.IsInitializedPropertyChanged);
        }

        /// <summary>
        /// When overridden in a derived class, asynchronously performs the initialization logic for the ViewModel.
        /// This method should contain any custom initialization logic required by the derived ViewModel class.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the initialization process.</param>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        protected virtual Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? Task.FromCanceled(cancellationToken) : Task.CompletedTask;
        }

        /// <summary>
        /// When overridden in a derived class, asynchronously performs the uninitialization logic for the ViewModel.
        /// This method should contain any custom uninitialization logic required by the derived ViewModel class.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the uninitialization process.</param>
        /// <returns>A task that represents the asynchronous uninitialization operation.</returns>
        protected virtual Task OnUninitializeAsync(CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? Task.FromCanceled(cancellationToken) : Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidParentViewModelAssignment()
        {
            throw new InvalidOperationException("ParentViewModel cannot be set to itself.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidThreadAccess()
        {
            throw new InvalidOperationException("The calling thread cannot access this object because a different thread owns it.");
        }

        /// <summary>
        /// Checks if the current thread is the same as the thread on which this instance was created and throws an <see cref="InvalidOperationException"/> if not.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the current thread is not the same as the thread on which this instance was created.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VerifyAccess()
        {
            if (!CheckAccess())
            {
                ThrowInvalidThreadAccess();
            }
        }

        #endregion
    }

    internal static partial class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs IsInitializedPropertyChanged = new(nameof(ViewModelBase.IsInitialized));
        internal static readonly PropertyChangedEventArgs ParameterPropertyChanged = new(nameof(ViewModelBase.Parameter));
        internal static readonly PropertyChangedEventArgs ParentViewModelPropertyChanged = new(nameof(ViewModelBase.ParentViewModel));
    }
}
