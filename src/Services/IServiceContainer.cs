using System;
using System.Collections.Generic;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Defines a container for registering and resolving service instances.
    /// </summary>
    public interface IServiceContainer : IServiceProvider
    {
        /// <summary>
        /// Gets a named service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <param name="name">
        /// The name of the service to resolve. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <returns>A service object of the specified type, or null if there is no such service.</returns>
        object? GetService(Type serviceType, string? name);

        /// <summary>
        /// Gets all registered services of a specified type.
        /// </summary>
        /// <param name="serviceType">The type of services to retrieve.</param>
        /// <returns>An enumerable of services of the specified type.</returns>
        IEnumerable<object> GetServices(Type serviceType);

        /// <summary>
        /// Registers a service instance under the specified type.
        /// </summary>
        /// <param name="serviceType">The type to register the service as.</param>
        /// <param name="service">The service instance to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, object service, bool throwIfExists = false);
        /// <summary>
        /// Registers a named service instance under the specified type.
        /// </summary>
        /// <param name="serviceType">The type to register the service as.</param>
        /// <param name="service">The service instance to register.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, object service, string? name, bool throwIfExists = false);

        /// <summary>
        /// Registers a service factory under the specified type.
        /// </summary>
        /// <param name="serviceType">The type to register the service as.</param>
        /// <param name="callback">The factory function to create the service instance.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, Func<object> callback, bool throwIfExists = false);
        /// <summary>
        /// Registers a named service factory under the specified type.
        /// </summary>
        /// <param name="serviceType">The type to register the service as.</param>
        /// <param name="callback">The factory function to create the service instance.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, Func<object> callback, string? name, bool throwIfExists = false);

        /// <summary>
        /// Registers a service factory where the implementation type differs from the registration type.
        /// </summary>
        /// <param name="serviceType">The type to register the service as (typically an interface or base class).</param>
        /// <param name="implementationType">The concrete type that will be created by the factory.</param>
        /// <param name="callback">The factory function to create the service instance.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, Type implementationType, Func<object> callback, bool throwIfExists = false);
        /// <summary>
        /// Registers a named service factory where the implementation type differs from the registration type.
        /// </summary>
        /// <param name="serviceType">The type to register the service as (typically an interface or base class).</param>
        /// <param name="implementationType">The concrete type that will be created by the factory.</param>
        /// <param name="callback">The factory function to create the service instance.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, Type implementationType, Func<object> callback, string? name, bool throwIfExists = false);

        /// <summary>
        /// Registers a service of the specified type by invoking its default constructor.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, bool throwIfExists = false);
        /// <summary>
        /// Registers a named service of the specified type by invoking its default constructor.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, string? name, bool throwIfExists = false);

        /// <summary>
        /// Registers a service type where the implementation type differs from the registration type.
        /// </summary>
        /// <param name="serviceType">The type to register the service as (typically an interface or base class).</param>
        /// <param name="implementationType">The concrete type to instantiate when resolving the service.</param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, Type implementationType, bool throwIfExists = false);
        /// <summary>
        /// Registers a named service type where the implementation type differs from the registration type.
        /// </summary>
        /// <param name="serviceType">The type to register the service as (typically an interface or base class).</param>
        /// <param name="implementationType">The concrete type to instantiate when resolving the service.</param>
        /// <param name="name">
        /// The name of the service to register. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <param name="throwIfExists">Specifies whether to throw an exception if the service already exists.</param>
        void RegisterService(Type serviceType, Type implementationType, string? name, bool throwIfExists = false);

        /// <summary>
        /// Unregisters a service instance.
        /// </summary>
        /// <param name="service">The service instance to unregister.</param>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        bool UnregisterService(object service);
        /// <summary>
        /// Unregisters a service of the specified type.
        /// </summary>
        /// <param name="serviceType">The type of the service to unregister.</param>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        bool UnregisterService(Type serviceType);
        /// <summary>
        /// Unregisters a named service of the specified type.
        /// </summary>
        /// <param name="serviceType">The type of the service to unregister.</param>
        /// <param name="name">
        /// The name of the service to unregister. This can be used to distinguish between multiple services of the same type.
        /// </param>
        /// <returns><c>true</c> if the service was successfully unregistered; otherwise, <c>false</c>.</returns>
        bool UnregisterService(Type serviceType, string? name);
        /// <summary>
        /// Removes all service registrations from this container.
        /// </summary>
        void Clear();
    }
}
