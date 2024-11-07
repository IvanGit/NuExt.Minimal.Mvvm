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
        private readonly record struct ServiceKey(Type Type, string? Name)
        {
            public readonly Type Type = Type;
            public readonly string? Name = Name;
        }

        private readonly struct ServiceValue
        {
            public readonly Lazy<object?> Service;

            public ServiceValue(object service)
            {
                Debug.Assert(service != null && service is not Func<object?> && service is not Type);
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
                Service = new Lazy<object?>(service);
#else
                Service = new Lazy<object?>(() => service);
                _ = Service.Value;
#endif
                Debug.Assert(Service.IsValueCreated);
            }

            public ServiceValue(Func<object?> callback)
            {
                Service = new Lazy<object?>(callback);
                Debug.Assert(!Service.IsValueCreated);
            }

            public ServiceValue(Type serviceType)
            {
                Service = new Lazy<object?>(() => Activator.CreateInstance(serviceType));
                Debug.Assert(!Service.IsValueCreated);
            }
        }

        private static readonly IServiceContainer s_default = new ServiceProvider();
        private static IServiceContainer? s_custom;

        private readonly ConcurrentDictionary<ServiceKey, ServiceValue> _services = new();

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
        public T? GetService<T>() where T : class
        {
            return (T?)GetService(typeof(T), (string?)null);
        }

        /// <summary>
        /// Gets a named service object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <param name="name">
        /// The name of the service to resolve. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <returns>A service object of type <typeparamref name="T"/> or null if there is no such service.</returns>
        public T? GetService<T>(string name) where T : class
        {
            return (T?)GetService(typeof(T), name);
        }

        /// <summary>
        /// Gets a service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of the specified type, or null if there is no such service.</returns>
        object? IServiceProvider.GetService(Type serviceType)
        {
            return GetService(serviceType, (string?)null);
        }

        /// <summary>
        /// Gets a named service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <param name="name">
        /// The name of the service to resolve. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <returns>A service object of the specified type, or null if there is no such service.</returns>
        public object? GetService(Type serviceType, string? name)
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

            var key = new ServiceKey(serviceType, !string.IsNullOrEmpty(name) ? name : null);
            if (_services.TryGetValue(key, out var serviceValue))
            {
                return serviceValue.Service.Value;
            }

            foreach (var pair in _services)
            {
                bool isApplicable = key.Name == null || key.Name == pair.Key.Name;
                if (!isApplicable)
                {
                    continue;
                }
                if (pair.Value.Service.IsValueCreated)
                {
                    if (serviceType.IsInstanceOfType(pair.Value.Service.Value))
                    {
                        return pair.Value.Service.Value;
                    }
                    continue;
                }
                if (serviceType.IsAssignableFrom(pair.Key.Type))
                {
                    return pair.Value.Service.Value;
                }
            }

            if (_parentProvider == null) return null;

            _isRecursive.Value = true;
            try
            {
                if (_parentProvider is ViewModelBase viewModelBase)
                {
                    return viewModelBase.GetService(serviceType, name);
                }
                return _parentProvider.GetService(serviceType);
            }
            finally
            {
                _isRecursive.Value = false;
            }
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
            Debug.Assert(!_isRecursive.Value);
            if (_isRecursive.Value)
            {
                yield break;
            }

            foreach (var pair in _services)
            {
                if (pair.Value.Service.IsValueCreated)
                {
                    if (serviceType.IsInstanceOfType(pair.Value.Service.Value))
                    {
                        yield return pair.Value.Service.Value;
                    }
                    continue;
                }
                if (serviceType.IsAssignableFrom(pair.Key.Type))
                {
                    yield return pair.Value.Service.Value;
                }
            }

            _isRecursive.Value = true;
            try
            {
                var services = new HashSet<object>();
                if (_parentProvider is ViewModelBase { ParentViewModel: ViewModelBase parentViewModel })
                {
                    foreach (var srv in parentViewModel.Services.GetServices(serviceType))
                    {
                        if (srv == null)
                        {
                            Debug.Fail($"Service is null for type {serviceType}");
                            continue;
                        }
                        services.Add(srv);
                    }
                }

                var service = _parentProvider?.GetService(serviceType);
                if (service != null)
                {
                    services.Add(service);
                }
                foreach (var srv in services)
                {
                    yield return srv;
                }
            }
            finally
            {
                _isRecursive.Value = false;
            }
        }

        #region Object registration

        /// <summary>
        /// Registers a service instance for a specific type.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="service">The service instance to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService<T>(T service, bool throwIfExists = false) where T:  class
        {
            RegisterService(typeof(T), service, (string?)null, throwIfExists);
        }

        /// <summary>
        /// Registers a named service instance for a specific type.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="service">The service instance to register.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService<T>(T service, string name, bool throwIfExists = false) where T : class
        {
            RegisterService(typeof(T), service, name, throwIfExists);
        }

        /// <summary>
        /// Registers a service instance.
        /// </summary>
        /// <param name="service">The service instance to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService(object service, bool throwIfExists = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(service);
#else
            _ = service ?? throw new ArgumentNullException(nameof(service));
#endif
            RegisterService(service.GetType(), service, (string?)null, throwIfExists);
        }

        /// <summary>
        /// Registers a named service instance.
        /// </summary>
        /// <param name="service">The service instance to register.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService(object service, string? name, bool throwIfExists = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(service);
#else
            _ = service ?? throw new ArgumentNullException(nameof(service));
#endif
            RegisterService(service.GetType(), service, name, throwIfExists);
        }

        /// <summary>
        /// Registers a service instance for a specific type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="service">The service instance to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService(Type serviceType, object service, bool throwIfExists = false)
        {
            RegisterService(serviceType, service, (string?)null, throwIfExists);
        }

        /// <summary>
        /// Registers a named service instance for a specific type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="service">The service instance to register.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService(Type serviceType, object service, string? name, bool throwIfExists = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceType);
            ArgumentNullException.ThrowIfNull(service);
#else
            _ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            _ = service ?? throw new ArgumentNullException(nameof(service));
#endif
            var key = new ServiceKey(serviceType, !string.IsNullOrEmpty(name) ? name : null);
            bool isExist = _services.TryGetValue(key, out _);
            if (throwIfExists && isExist)
            {
                ThrowServiceExistsException(serviceType);
            }
            if (isExist)
            {
                UnregisterService(key);
            }
            _services[key] = new ServiceValue(service);
        }

        #endregion

        #region Factory registration

        /// <summary>
        /// Registers a service with a callback function for a specific type.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="callback">The callback function to create the service instance.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService<T>(Func<T> callback, bool throwIfExists = false)
        {
            try
            {
                RegisterService(typeof(T), (Func<object>)(object)callback, (string?)null, throwIfExists);
            }
            catch (InvalidCastException)
            {
                RegisterService(typeof(T), new Func<object>(() => callback()!), (string?)null, throwIfExists);
            }
        }

        /// <summary>
        /// Registers a named service with a callback function for a specific type.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="callback">The callback function to create the service instance.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService<T>(Func<T> callback, string name, bool throwIfExists = false)
        {
            try
            {
                RegisterService(typeof(T), (Func<object>)(object)callback, name, throwIfExists);
            }
            catch (InvalidCastException)
            {
                RegisterService(typeof(T), new Func<object>(() => callback()!), name, throwIfExists);
            }
        }

        /// <summary>
        /// Registers a service with a callback function.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="callback">The callback function to create the service instance.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService(Type serviceType, Func<object> callback, bool throwIfExists = false)
        {
            RegisterService(serviceType, callback, (string?)null, throwIfExists);
        }

        /// <summary>
        /// Registers a named service with a callback function.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="callback">The callback function to create the service instance.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService(Type serviceType, Func<object> callback, string? name, bool throwIfExists = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceType);
            ArgumentNullException.ThrowIfNull(callback);
#else
            _ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            _ = callback ?? throw new ArgumentNullException(nameof(callback));
#endif
            var key = new ServiceKey(serviceType, !string.IsNullOrEmpty(name) ? name : null);
            bool isExist = _services.TryGetValue(key, out _);
            if (throwIfExists && isExist)
            {
                ThrowServiceExistsException(serviceType);
            }
            if (isExist)
            {
                UnregisterService(key);
            }
            _services[key] = new ServiceValue(callback);
        }

        #endregion

        #region Type registration

        /// <summary>
        /// Registers a service of the specified type by invoking its default constructor.
        /// </summary>
        /// <typeparam name="T">The type of the service to register.</typeparam>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService<T>(bool throwIfExists = false) where T : class
        {
            RegisterService(typeof(T), (string?)null, throwIfExists);
        }

        /// <summary>
        /// Registers a named service of the specified type by invoking its default constructor.
        /// </summary>
        /// <typeparam name="T">The type of the service to register.</typeparam>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService<T>(string? name, bool throwIfExists = false) where T : class
        {
            RegisterService(typeof(T), name, throwIfExists);
        }

        /// <summary>
        /// Registers a service of the specified type by invoking its default constructor.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService(Type serviceType, bool throwIfExists = false)
        {
            RegisterService(serviceType, (string?)null, throwIfExists);
        }

        /// <summary>
        /// Registers a named service of the specified type by invoking its default constructor.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public void RegisterService(Type serviceType, string? name, bool throwIfExists = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceType);
#else
            _ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
#endif
            var key = new ServiceKey(serviceType, !string.IsNullOrEmpty(name) ? name : null);
            bool isExist = _services.TryGetValue(key, out _);
            if (throwIfExists && isExist)
            {
                ThrowServiceExistsException(serviceType);
            }
            if (isExist)
            {
                UnregisterService(key);
            }
            _services[key] = new ServiceValue(serviceType);
        }

        #endregion

        private static void ThrowServiceExistsException(Type serviceType)
        {
            throw new InvalidOperationException($"Service of type {serviceType} is already registered.");
        }

        /// <summary>
        /// Unregisters a service instance.
        /// </summary>
        /// <param name="service">The service instance to unregister.</param>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        public bool UnregisterService(object service)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(service);
#else
            _ = service ?? throw new ArgumentNullException(nameof(service));
#endif
            ServiceKey? key = null;
            foreach (var pair in _services)
            {
                if (!pair.Value.Service.IsValueCreated) continue;
                if (pair.Value.Service.Value != service) continue;
                key = pair.Key;
                break;
            }
            return key.HasValue && UnregisterService(key.Value);
        }

        /// <summary>
        /// Unregisters a service of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the service to unregister.</typeparam>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        public bool UnregisterService<T>() where T : class
        {
            return UnregisterService(typeof(T), (string?)null);
        }

        /// <summary>
        /// Unregisters a named service of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the service to unregister.</typeparam>
        /// <param name="name">
        /// The name of the service to unregister. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        public bool UnregisterService<T>(string name) where T : class
        {
            return UnregisterService(typeof(T), name);
        }

        /// <summary>
        /// Unregisters a service of the specified type.
        /// </summary>
        /// <param name="serviceType">The type of the service to unregister.</param>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        public bool UnregisterService(Type serviceType)
        {
            return UnregisterService(serviceType, (string?)null);
        }

        /// <summary>
        /// Unregisters a named service of the specified type.
        /// </summary>
        /// <param name="serviceType">The type of the service to unregister.</param>
        /// <param name="name">
        /// The name of the service to unregister. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        public bool UnregisterService(Type serviceType, string? name)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceType);
#else
            _ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
#endif
            var key = new ServiceKey(serviceType, !string.IsNullOrEmpty(name) ? name : null);
            return UnregisterService(key);
        }

        private bool UnregisterService(ServiceKey key)
        {
            return _services.TryRemove(key, out _);
        }

        #endregion
    }
}
