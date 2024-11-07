#if NETFRAMEWORK || WINDOWS
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents an abstract base class that provides behavior functionality for WPF elements of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the associated object, which must be a DependencyObject.</typeparam>
    public abstract class Behavior<T> : Behavior where T : DependencyObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Behavior{T}"/> class.
        /// </summary>
        protected Behavior() : base(typeof(T))
        {
        }

        #region Properties

        /// <summary>
        /// Gets the object to which this behavior is attached, cast to the specified type <typeparamref name="T"/>.
        /// </summary>
        protected new T? AssociatedObject => (T?)base.AssociatedObject;

        #endregion
    }
}
#endif