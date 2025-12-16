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
    public class ServiceProvider : IServiceContainer
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
        private readonly ThreadLocal<bool> _isRecursive = new();

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

        /// <inheritdoc />
        public T? GetService<T>() where T : class
        {
            return (T?)GetService(serviceType: typeof(T), name: null);
        }

        /// <inheritdoc />
        public T? GetService<T>(string? name) where T : class
        {
            return (T?)GetService(serviceType: typeof(T), name: name);
        }

        /// <inheritdoc />
        object? IServiceProvider.GetService(Type serviceType)
        {
            return GetService(serviceType: serviceType, name: null);
        }

        /// <inheritdoc />
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

            /* priorities:
            * 1. exact match (type+name)
            * 2. if named (name != null):
            *      2.1 local search name match & type assignment
            *      2.2 parent search
            *      2.3 exit (no fallback)
            * 3. if unnamed (name == null):
            *      3.1 local search by null name & type assignment
            *      3.2 local search by non-null name & type assignment
            *      3.3 parent search
            *      3.4 exit
            */

            var key = new ServiceKey(serviceType, !string.IsNullOrEmpty(name) ? name : null);

            //1. exact match (type+name)
            if (_services.TryGetValue(key, out var serviceValue))
            {
                return serviceValue.Service.Value;
            }

            //2. if named (name != null):
            if (key.Name != null)
            {
                //2.1 local search name match & type assignment
                foreach (var pair in _services)
                {
                    if (key.Name != pair.Key.Name) continue;
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
            }
            else //3. if unnamed (name == null):
            {
                //3.1 local search by null name & type assignment
                foreach (var pair in _services)
                {
                    if (pair.Key.Name != null) continue;
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

                //3.2 local search by non-null name & type assignment
                foreach (var pair in _services)
                {
                    if (pair.Key.Name == null) continue;
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

            }

            //2.2 parent search
            //3.3 parent search

            if (_parentProvider == null) return null;

            var viewModelBase = _parentProvider as ViewModelBase;
            if (viewModelBase == null && key.Name != null) return null;

            _isRecursive.Value = true;
            try
            {
                if (viewModelBase != null)
                {
                    return viewModelBase.GetService(serviceType, key.Name);
                }
                return key.Name == null ? _parentProvider.GetService(serviceType) : null;
            }
            finally
            {
                _isRecursive.Value = false;
            }
        }

        /// <inheritdoc />
        public IEnumerable<T> GetServices<T>() where T : class
        {
            foreach (object? obj in GetServices(typeof(T)))
            {
                yield return (T)obj!;
            }
        }

        /// <inheritdoc />
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

            var services = new HashSet<object>();

            foreach (var pair in _services)
            {
                if (pair.Value.Service.IsValueCreated)
                {
                    var value = pair.Value.Service.Value;
                    if (value != null && serviceType.IsInstanceOfType(value) && services.Add(value))
                    {
                        yield return value;
                    }
                    continue;
                }
                if (serviceType.IsAssignableFrom(pair.Key.Type))
                {
                    var value = pair.Value.Service.Value;
                    if (value != null && services.Add(value))
                    {
                        yield return value;
                    }
                }
            }

            if (_parentProvider == null) yield break;

            _isRecursive.Value = true;
            try
            {
                if (_parentProvider is ViewModelBase { ParentViewModel: ViewModelBase parentViewModel })
                {
                    foreach (var service in parentViewModel.Services.GetServices(serviceType))
                    {
                        Debug.Assert(service != null, $"Service is null for type {serviceType}");
                        if (service != null && services.Add(service))
                        {
                            yield return service;
                        }
                    }
                }
                else
                {
                    var service = _parentProvider.GetService(serviceType);
                    if (service != null && services.Add(service))
                    {
                        yield return service;
                    }
                }
            }
            finally
            {
                _isRecursive.Value = false;
            }
        }

        #region Instance registration

        /// <inheritdoc />
        public void RegisterService<T>(T service, bool throwIfExists = false) where T : class
        {
            RegisterService(serviceType: typeof(T), service: service, name: null, throwIfExists);
        }

        /// <inheritdoc />
        public void RegisterService<T>(T service, string? name, bool throwIfExists = false) where T : class
        {
            RegisterService(serviceType: typeof(T), service: service, name: name, throwIfExists);
        }

        /// <inheritdoc />
        public void RegisterService(Type serviceType, object service, bool throwIfExists = false)
        {
            RegisterService(serviceType: serviceType, service: service, name: null, throwIfExists);
        }

        /// <inheritdoc />
        public void RegisterService(Type serviceType, object service, string? name, bool throwIfExists = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceType);
            ArgumentNullException.ThrowIfNull(service);
#else
            _ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            _ = service ?? throw new ArgumentNullException(nameof(service));
#endif
            if (!serviceType.IsInstanceOfType(service))
            {
                throw new ArgumentException($"Service must be assignable to {serviceType}", nameof(service));
            }

            var key = new ServiceKey(serviceType, !string.IsNullOrEmpty(name) ? name : null);
            RegisterServiceCore(key, new ServiceValue(service), throwIfExists);
        }

        #endregion

        #region Factory registration

        /// <inheritdoc />
        public void RegisterService<T>(Func<T> callback, bool throwIfExists = false) where T : class
        {
            try
            {
                RegisterService(serviceType: typeof(T), callback: (Func<object>)(object)callback, name: null, throwIfExists);
            }
            catch (InvalidCastException)
            {
                RegisterService(serviceType: typeof(T), callback: new Func<object>(() => callback()!), name: null, throwIfExists);
            }
        }

        /// <inheritdoc />
        public void RegisterService<T>(Func<T> callback, string? name, bool throwIfExists = false) where T : class
        {
            try
            {
                RegisterService(serviceType: typeof(T), callback: (Func<object>)(object)callback, name: name, throwIfExists);
            }
            catch (InvalidCastException)
            {
                RegisterService(serviceType: typeof(T), callback: new Func<object>(() => callback()!), name: name, throwIfExists);
            }
        }

        /// <inheritdoc />
        public void RegisterService(Type serviceType, Func<object> callback, bool throwIfExists = false)
        {
            RegisterService(serviceType: serviceType, callback: callback, name: null, throwIfExists);
        }

        /// <inheritdoc />
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
            RegisterServiceCore(key, new ServiceValue(callback), throwIfExists);
        }

        #endregion

        #region Type registration

        /// <inheritdoc />
        public void RegisterService<T>(bool throwIfExists = false) where T : class
        {
            RegisterService(serviceType: typeof(T), name: null, throwIfExists);
        }

        /// <inheritdoc />
        public void RegisterService<T>(string? name, bool throwIfExists = false) where T : class
        {
            RegisterService(serviceType: typeof(T), name: name, throwIfExists);
        }

        /// <inheritdoc />
        public void RegisterService(Type serviceType, bool throwIfExists = false)
        {
            RegisterService(serviceType: serviceType, name: null, throwIfExists);
        }

        /// <inheritdoc />
        public void RegisterService(Type serviceType, string? name, bool throwIfExists = false)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(serviceType);
#else
            _ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
#endif
            var key = new ServiceKey(serviceType, !string.IsNullOrEmpty(name) ? name : null);
            RegisterServiceCore(key, new ServiceValue(serviceType), throwIfExists);
        }

        #endregion

        private void RegisterServiceCore(ServiceKey key, ServiceValue value, bool throwIfExists)
        {
            if (throwIfExists)
            {
                if (!_services.TryAdd(key, value))
                {
                    ThrowServiceExistsException(key.Type);
                }
                return;
            }

            ServiceValue? replacedValue = null;
            _services.AddOrUpdate(key, value, (k, oldValue) => { replacedValue = oldValue; return value; });
            if (replacedValue.HasValue)
            {
                OnServiceRemoved(key, replacedValue.Value);
            }
        }

        private static void OnServiceRemoved(ServiceKey key, ServiceValue value)
        {
            if (!value.Service.IsValueCreated) return;

            if (value.Service.Value is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    var message = $"Dispose failed for service ['{key.Name ?? "(unnamed)"}', {key.Type}]: {ex.Message}";
                    Debug.Fail(message);
                    Trace.WriteLine(message);
                }
            }
        }

        private static void ThrowServiceExistsException(Type serviceType)
        {
            throw new InvalidOperationException($"Service of type {serviceType} is already registered.");
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool UnregisterService<T>() where T : class
        {
            return UnregisterService(serviceType: typeof(T), name: null);
        }

        /// <inheritdoc />
        public bool UnregisterService<T>(string name) where T : class
        {
            return UnregisterService(serviceType: typeof(T), name: name);
        }

        /// <inheritdoc />
        public bool UnregisterService(Type serviceType)
        {
            return UnregisterService(serviceType: serviceType, name: null);
        }

        /// <inheritdoc />
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
            if (!_services.TryRemove(key, out var value)) return false;
            OnServiceRemoved(key, value);
            return true;
        }

        /// <inheritdoc />
        public void Clear()
        {
            var entries = _services.ToArray();
            _services.Clear();

            foreach (var entry in entries)
            {
                OnServiceRemoved(entry.Key, entry.Value);
            }
        }

        #endregion
    }
}
