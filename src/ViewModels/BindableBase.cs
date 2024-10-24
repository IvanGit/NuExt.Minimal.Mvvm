using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Minimal.Mvvm
{
    /// <summary>
    /// A base class that implements <see cref="INotifyPropertyChanged"/> to simplify models.
    /// </summary>
    [DataContract]
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
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="changedCallback">Optional callback to invoke after the value has been changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, string? propertyName, Action? changedCallback)
        {
            if (!SetProperty(ref storage, value, propertyName, out _)) return false;
            changedCallback?.Invoke();
            return true;
        }

        /// <summary>
        /// Sets the property, raises the <see cref="PropertyChanged"/> event, and invokes a callback with the old value if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the property's backing field.</param>
        /// <param name="value">New value to set.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="changedCallback">Optional callback to invoke with the old value after the value has been changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, string? propertyName, Action<T>? changedCallback)
        {
            if (!SetProperty(ref storage, value, propertyName, out var oldValue)) return false;
            changedCallback?.Invoke(oldValue);
            return true;
        }

        /// <summary>
        /// Sets the property, raises the <see cref="PropertyChanged"/> event, and outputs the old value if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the property's backing field.</param>
        /// <param name="value">New value to set.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, string? propertyName, out T oldValue)
        {
            oldValue = storage;
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
