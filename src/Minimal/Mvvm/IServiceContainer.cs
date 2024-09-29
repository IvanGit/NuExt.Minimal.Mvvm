using System;
using System.Collections.Generic;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Defines a container for registering and resolving service instances.
    /// </summary>
    public interface IServiceContainer: IServiceProvider
    {
        /// <summary>
        /// Gets a service object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <returns>A service object of type <typeparamref name="T"/> or null if there is no such service.</returns>
        T? GetService<T>();

        /// <summary>
        /// Gets all registered services of a specified type.
        /// </summary>
        /// <param name="serviceType">The type of services to retrieve.</param>
        /// <returns>An enumerable of services of the specified type.</returns>
        IEnumerable<object?> GetServices(Type serviceType);

        /// <summary>
        /// Registers a service instance.
        /// </summary>
        /// <param name="service">The service instance to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(object? service, bool throwIfExists = false);

        /// <summary>
        /// Registers a service of the specified type by invoking its default constructor.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, bool throwIfExists = false);

        /// <summary>
        /// Registers a service with a callback function.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="callback">The callback function to create the service instance.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, Func<object?> callback, bool throwIfExists = false);

        /// <summary>
        /// Registers a service instance for a specific type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="service">The service instance to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, object? service, bool throwIfExists = false);

        /// <summary>
        /// Registers a service of the specified type by invoking its default constructor.
        /// </summary>
        /// <typeparam name="T">The type of the service to register.</typeparam>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService<T>(bool throwIfExists = false);

        /// <summary>
        /// Registers a service with a callback function for a specific type.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="callback">The callback function to create the service instance.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService<T>(Func<T?> callback, bool throwIfExists = false);

        /// <summary>
        /// Registers a service instance for a specific type.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="service">The service instance to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService<T>(T service, bool throwIfExists = false);

        /// <summary>
        /// Unregisters a service of the specified type.
        /// </summary>
        /// <param name="serviceType">The type of the service to unregister.</param>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        bool UnregisterService(Type serviceType);

        /// <summary>
        /// Unregisters a service of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the service to unregister.</typeparam>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        bool UnregisterService<T>();
    }
}
