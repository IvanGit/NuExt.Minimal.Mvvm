using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Minimal.Mvvm
{
    /// <summary>
    /// A base class that implements <see cref="INotifyPropertyChanged"/> to simplify models.
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        public void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for multiple properties.
        /// </summary>
        /// <param name="propertyNames">An array of property names.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyNames"/> is <c>null</c>.</exception>
        protected void RaisePropertiesChanged(params string[] propertyNames)
        {
            _ = propertyNames ?? throw new ArgumentNullException(nameof(propertyNames));
            foreach (string propertyName in propertyNames)
            {
                RaisePropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the property's backing field.</param>
        /// <param name="value">New value to set.</param>
        /// <param name="propertyName">Name of the property. This optional parameter is automatically provided by the compiler.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            return SetProperty<T>(ref storage, value, propertyName, (Action?)null);
        }

        /// <summary>
        /// Sets the property, raises the <see cref="PropertyChanged"/> event, and invokes a callback if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the property's backing field.</param>
        /// <param name="value">New value to set.</param>
        /// <param name="propertyName">Name of the property. This optional parameter is automatically provided by the compiler.</param>
        /// <param name="changedCallback">Optional callback to invoke after the value has been changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, string? propertyName, Action? changedCallback)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            changedCallback?.Invoke();
            return true;
        }

        /// <summary>
        /// Sets the property, raises the <see cref="PropertyChanged"/> event, and invokes a callback with the old value if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the property's backing field.</param>
        /// <param name="value">New value to set.</param>
        /// <param name="propertyName">Name of the property. This optional parameter is automatically provided by the compiler.</param>
        /// <param name="changedCallback">Optional callback to invoke with the old value after the value has been changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, string? propertyName, Action<T>? changedCallback)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            T oldValue = storage;
            storage = value;
            OnPropertyChanged(propertyName);
            changedCallback?.Invoke(oldValue);
            return true;
        }

        #endregion
    }
}
