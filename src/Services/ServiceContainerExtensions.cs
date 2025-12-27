using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Minimal.Mvvm
{
    public static class ServiceContainerExtensions
    {
        /// <summary>
        /// Gets a service object of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service object to get.</typeparam>
        /// <param name="container">The service container.</param>
        /// <returns>A service object of type <typeparamref name="TService"/> or null if there is no such service.</returns>
        public static TService? GetService<TService>(this IServiceContainer container) where TService : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            return container.GetService<TService>(name: null);
        }

        /// <summary>
        /// Gets a named service object of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service object to get.</typeparam>
        /// <param name="container">The service container.</param>
        /// <param name="name">
        /// The name of the service to resolve. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <returns>A service object of type <typeparamref name="TService"/> or null if there is no such service.</returns>
        public static TService? GetService<TService>(this IServiceContainer container, string? name) where TService : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            return (TService?)container.GetService(serviceType: typeof(TService), name: name);
        }

        /// <summary>
        ///  Gets all registered services of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service object to get.</typeparam>
        /// <param name="container">The service container.</param>
        /// <returns>An enumerable of services of type <typeparamref name="TService"/>.</returns>
        public static IEnumerable<TService> GetServices<TService>(this IServiceContainer container) where TService : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            foreach (var service in container.GetServices(typeof(TService)))
            {
                yield return (TService)service;
            }
        }

        /// <summary>
        /// Registers a service instance under the specified type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type to register the service as.</typeparam>
        /// <param name="container">The service container.</param>
        /// <param name="service">The service instance to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public static void RegisterService<TService>(this IServiceContainer container, TService service, bool throwIfExists = false) where TService : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            container.RegisterService<TService>(service: service, name: null, throwIfExists);
        }

        /// <summary>
        /// Registers a named service instance under the specified type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type to register the service as.</typeparam>
        /// <param name="container">The service container.</param>
        /// <param name="service">The service instance to register.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public static void RegisterService<TService>(this IServiceContainer container, TService service, string? name, bool throwIfExists = false) where TService : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            container.RegisterService(serviceType: typeof(TService), service: service, name: name, throwIfExists);
        }

        /// <summary>
        /// Registers a service factory under the specified type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type to register the service as.</typeparam>
        /// <param name="container">The service container.</param>
        /// <param name="callback">The factory function to create the service instance.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public static void RegisterService<TService>(this IServiceContainer container, Func<TService> callback, bool throwIfExists = false) where TService : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            container.RegisterService(callback: callback, name: null, throwIfExists);
        }

        /// <summary>
        /// Registers a named service factory under the specified type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type to register the service as.</typeparam>
        /// <param name="container">The service container.</param>
        /// <param name="callback">The factory function to create the service instance.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public static void RegisterService<TService>(this IServiceContainer container, Func<TService> callback, string? name, bool throwIfExists = false) where TService : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            try
            {
                container.RegisterService(serviceType: typeof(TService), callback: (Func<object>)(object)callback, name: name, throwIfExists);
            }
            catch (InvalidCastException ex)
            {
                Trace.WriteLine("RegisterService: " + ex.Message);
                Debug.Fail(ex.Message);
                container.RegisterService(serviceType: typeof(TService), callback: new Func<object>(() => callback()), name: name, throwIfExists);
            }
        }

        /// <summary>
        /// Registers a service factory where the implementation type differs from the registration type.
        /// </summary>
        /// <typeparam name="TService">The type to register the service as (typically an interface or base class).</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be created by the factory.</typeparam>
        /// <param name="container">The service container.</param>
        /// <param name="callback">The factory function to create the service instance.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public static void RegisterService<TService, TImplementation>(this IServiceContainer container, Func<TImplementation> callback, bool throwIfExists = false)
            where TService : class
            where TImplementation : class, TService
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            container.RegisterService<TService, TImplementation>(callback: callback, name: null, throwIfExists);
        }

        /// <summary>
        /// Registers a named service factory where the implementation type differs from the registration type.
        /// </summary>
        /// <typeparam name="TService">The type to register the service as (typically an interface or base class).</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be created by the factory.</typeparam>
        /// <param name="container">The service container.</param>
        /// <param name="callback">The factory function to create the service instance.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public static void RegisterService<TService, TImplementation>(this IServiceContainer container, Func<TImplementation> callback, string? name, bool throwIfExists = false)
            where TService : class
            where TImplementation : class, TService
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            try
            {
                container.RegisterService(serviceType: typeof(TService), implementationType: typeof(TImplementation), callback: (Func<object>)(object)callback, name: name, throwIfExists);
            }
            catch (InvalidCastException ex)
            {
                Trace.WriteLine("RegisterService: " + ex.Message);
                Debug.Fail(ex.Message);
                container.RegisterService(serviceType: typeof(TService), implementationType: typeof(TImplementation), callback: new Func<object>(() => callback()), name: name, throwIfExists);
            }
        }

        /// <summary>
        /// Registers a service of the specified type <typeparamref name="TService"/> by invoking its default constructor.
        /// </summary>
        /// <typeparam name="TService">The type of the service to register.</typeparam>
        /// <param name="container">The service container.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public static void RegisterService<TService>(this IServiceContainer container, bool throwIfExists = false) where TService : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            container.RegisterService<TService>(name: null, throwIfExists);
        }

        /// <summary>
        /// Registers a named service of the specified type <typeparamref name="TService"/> by invoking its default constructor.
        /// </summary>
        /// <typeparam name="TService">The type of the service to register.</typeparam>
        /// <param name="container">The service container.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public static void RegisterService<TService>(this IServiceContainer container, string? name, bool throwIfExists = false) where TService : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            container.RegisterService(serviceType: typeof(TService), name: name, throwIfExists);
        }

        /// <summary>
        /// Registers a service type where the implementation type differs from the registration type.
        /// </summary>
        /// <typeparam name="TService">The type to register the service as (typically an interface or base class).</typeparam>
        /// <typeparam name="TImplementation">The concrete type to instantiate when resolving the service.</typeparam>
        /// <param name="container">The service container.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public static void RegisterService<TService, TImplementation>(this IServiceContainer container, bool throwIfExists = false)
   where TService : class
   where TImplementation : class, TService
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            container.RegisterService<TService, TImplementation>(name: null, throwIfExists);
        }

        /// <summary>
        /// Registers a named service type where the implementation type differs from the registration type.
        /// </summary>
        /// <typeparam name="TService">The type to register the service as (typically an interface or base class).</typeparam>
        /// <typeparam name="TImplementation">The concrete type to instantiate when resolving the service.</typeparam>
        /// <param name="container">The service container.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        public static void RegisterService<TService, TImplementation>(this IServiceContainer container, string? name, bool throwIfExists = false)
            where TService : class
            where TImplementation : class, TService
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            container.RegisterService(serviceType: typeof(TService), implementationType: typeof(TImplementation), name: name, throwIfExists);
        }

        /// <summary>
        /// Unregisters a service of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to unregister.</typeparam>
        /// <param name="container">The service container.</param>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        public static bool UnregisterService<TService>(this IServiceContainer container) where TService : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            return container.UnregisterService<TService>(name: null);
        }

        /// <summary>
        /// Unregisters a named service of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to unregister.</typeparam>
        /// <param name="container">The service container.</param>
        /// <param name="name">
        /// The name of the service to unregister. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        public static bool UnregisterService<TService>(this IServiceContainer container, string? name) where TService : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(container);
#else
            _ = container ?? throw new ArgumentNullException(nameof(container));
#endif
            return container.UnregisterService(serviceType: typeof(TService), name: name);
        }
    }
}
