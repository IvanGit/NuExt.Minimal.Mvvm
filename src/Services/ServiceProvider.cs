using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Provides a simple implementation of a service container which supports 
    /// registering and retrieving services with optional parent provider support.
    /// </summary>
    public class ServiceProvider: IServiceContainer
    {
        private readonly struct ServiceRegistration
        {
            internal readonly Lazy<object?> Service;

            public ServiceRegistration(object service)
            {
                Debug.Assert(service != null && service is not Func<object?> && service is not Type);
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
                Service = new Lazy<object?>(service);
#else
                Service = new Lazy<object?>(() => service);
#endif
            }

            public ServiceRegistration(Func<object?> callback)
            {
                Service = new Lazy<object?>(callback);
            }

            public ServiceRegistration(Type serviceType)
            {
                Service = new Lazy<object?>(() => Activator.CreateInstance(serviceType));
            }
        }

        private static readonly IServiceContainer s_default = new ServiceProvider();
        private static IServiceContainer? s_custom;

        private readonly ConcurrentDictionary<Type, ServiceRegistration> _services = new();

        private readonly IServiceProvider? _parentProvider;

        private readonly AsyncLocal<bool> _isRecursive = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProvider"/> class.
        /// </summary>
        public ServiceProvider()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProvider"/> class with a parent service provider.
        /// </summary>
        /// <param name="parentServiceProvider">The parent service provider.</param>
        public ServiceProvider(IServiceProvider parentServiceProvider)
        {
            _parentProvider = parentServiceProvider ?? throw new ArgumentNullException(nameof(parentServiceProvider));
        }

        #region Properties

        /// <summary>
        /// Gets or sets the default service provider instance.
        /// </summary>
        public static IServiceContainer Default
        {
            get => s_custom ?? s_default;
            set => s_custom = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a service object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <returns>A service object of type <typeparamref name="T"/> or null if there is no such service.</returns>
        public T? GetService<T>()
        {
            return (T?)((IServiceProvider)this).GetService(typeof(T));
        }

        /// <summary>
        /// Gets all registered services of a specified type.
        /// </summary>
        /// <param name="serviceType">The type of services to retrieve.</param>
        /// <returns>An enumerable of services of the specified type.</returns>
        public IEnumerable<object?> GetServices(Type serviceType)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceType);
#else
            _ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
#endif
            foreach (var pair in _services)
            {
                if (serviceType.IsInstanceOfType(pair.Value.Service.Value))
                {
                    yield return pair.Value.Service.Value;
                }
            }
        }

        /// <summary>
        /// Gets a service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of the specified type, or null if there is no such service.</returns>
        object? IServiceProvider.GetService(Type serviceType)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceType);
#else
           _ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
#endif
            if (_isRecursive.Value)
            {
                return null;
            }
            _isRecursive.Value = true;
            try
            {
                if (_services.TryGetValue(serviceType, out var serviceRegistration))
                {
                    return serviceRegistration.Service.Value;
                }
                return _parentProvider?.GetService(serviceType);
            }
            finally
            {
                _isRecursive.Value = false;
            }
        }

        /// <summary>
        /// Registers a service instance.
        /// </summary>
        /// <param name="service">The service instance to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService(object? service, bool throwIfExists = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(service);
#else
            _ = service ?? throw new ArgumentNullException(nameof(service));
#endif
            RegisterService(service.GetType(), service, throwIfExists);
        }

        /// <summary>
        /// Registers a service of the specified type by invoking its default constructor.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService(Type serviceType, bool throwIfExists = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceType);
#else
            _ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
#endif
            if (throwIfExists && _services.ContainsKey(serviceType))
            {
                ThrowServiceExistsException(serviceType);
            }
            _services[serviceType] = new ServiceRegistration(serviceType);
        }

        /// <summary>
        /// Registers a service with a callback function.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="callback">The callback function to create the service instance.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService(Type serviceType, Func<object?> callback, bool throwIfExists = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceType);
            ArgumentNullException.ThrowIfNull(callback);
#else
            _ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            _ = callback ?? throw new ArgumentNullException(nameof(callback));
#endif
            if (throwIfExists && _services.ContainsKey(serviceType))
            {
                ThrowServiceExistsException(serviceType);
            }
            _services[serviceType] = new ServiceRegistration(callback);
        }

        /// <summary>
        /// Registers a service instance for a specific type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="service">The service instance to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService(Type serviceType, object? service, bool throwIfExists = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceType);
            ArgumentNullException.ThrowIfNull(service);
#else
            _ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            _ = service ?? throw new ArgumentNullException(nameof(service));
#endif
            if (throwIfExists && _services.ContainsKey(serviceType))
            {
                ThrowServiceExistsException(serviceType);
            }
            _services[serviceType] = new ServiceRegistration(service);
        }

        /// <summary>
        /// Registers a service of the specified type by invoking its default constructor.
        /// </summary>
        /// <typeparam name="T">The type of the service to register.</typeparam>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService<T>(bool throwIfExists = false)
        {
            RegisterService(typeof(T), throwIfExists);
        }

        /// <summary>
        /// Registers a service with a callback function for a specific type.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="callback">The callback function to create the service instance.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService<T>(Func<T?> callback, bool throwIfExists = false)
        {
            try
            {
                RegisterService(typeof(T), (Func<object?>)(object)callback, throwIfExists);
            }
            catch (InvalidCastException)
            {
                RegisterService(typeof(T), new Func<object?>(() => callback()), throwIfExists);
            }
        }

        /// <summary>
        /// Registers a service instance for a specific type.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="service">The service instance to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService<T>(T service, bool throwIfExists = false)
        {
            RegisterService(typeof(T), service, throwIfExists);
        }

        private static void ThrowServiceExistsException(Type serviceType)
        {
            throw new InvalidOperationException($"Service of type {serviceType} is already registered.");
        }

        /// <summary>
        /// Unregisters a service of the specified type.
        /// </summary>
        /// <param name="serviceType">The type of the service to unregister.</param>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        public bool UnregisterService(Type serviceType)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceType);
#else
            _ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
#endif

            return _services.TryRemove(serviceType, out _);
        }

        /// <summary>
        /// Unregisters a service of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the service to unregister.</typeparam>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        public bool UnregisterService<T>()
        {
            return UnregisterService(typeof(T));
        }

        #endregion
    }
}
