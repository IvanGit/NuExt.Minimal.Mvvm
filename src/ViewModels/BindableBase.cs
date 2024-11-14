using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Minimal.Mvvm
{
    /// <summary>
    /// A base class that implements <see cref="INotifyPropertyChanged"/> to simplify models.
    /// This class provides support for notifying clients that a property value has changed, and supports the cancellation of property value changes.
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
        /// Determines whether the property can be set. Can be overridden in derived classes for custom logic.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="oldValue">The current value of the property.</param>
        /// <param name="newValue">The new value to set.</param>
        /// <param name="propertyName">The name of the property being updated. This is optional and can be automatically provided by the compiler.</param>
        /// <returns>True if the property can be set; otherwise, false.</returns>
        protected virtual bool CanSetProperty<T>(T oldValue, T newValue, [CallerMemberName] string? propertyName = null)
        {
            return true;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for multiple properties.
        /// </summary>
        /// <param name="propertyNames">An array of property names.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyNames"/> is <c>null</c>.</exception>
        public void RaisePropertiesChanged(params string[] propertyNames)
        {
            _ = propertyNames ?? throw new ArgumentNullException(nameof(propertyNames));
            foreach (string propertyName in propertyNames)
            {
                RaisePropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for three specified properties.
        /// </summary>
        /// <param name="propertyName1">The name of the first property that changed.</param>
        /// <param name="propertyName2">The name of the second property that changed.</param>
        /// <param name="propertyName3">The name of the third property that changed.</param>
        public void RaisePropertiesChanged(string propertyName1, string propertyName2, string propertyName3)
        {
            OnPropertyChanged(propertyName1);
            OnPropertyChanged(propertyName2);
            OnPropertyChanged(propertyName3);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for two specified properties.
        /// </summary>
        /// <param name="propertyName1">The name of the first property that changed.</param>
        /// <param name="propertyName2">The name of the second property that changed.</param>
        public void RaisePropertiesChanged(string propertyName1, string propertyName2)
        {
            OnPropertyChanged(propertyName1);
            OnPropertyChanged(propertyName2);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        public void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the property's backing field.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property. This optional parameter is automatically provided by the compiler.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            return SetProperty(ref storage, value, (Action?)null, propertyName);
        }

        /// <summary>
        /// Sets the property, raises the <see cref="PropertyChanged"/> event, and invokes a callback if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the property's backing field.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="changedCallback">Callback to invoke after the value has been changed.</param>
        /// <param name="propertyName">The name of the property. This optional parameter is automatically provided by the compiler.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action? changedCallback, [CallerMemberName] string? propertyName = null)
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
        /// <param name="value">The new value to set.</param>
        /// <param name="changedCallback">Callback to invoke with the old value after the value has been changed.</param>
        /// <param name="propertyName">The name of the property. This optional parameter is automatically provided by the compiler.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, Action<T>? changedCallback, [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref storage, value, propertyName, out var oldValue)) return false;
            changedCallback?.Invoke(oldValue);
            return true;
        }

        /// <summary>
        /// Updates the specified property if the new value is different from the current value,
        /// raises the <see cref="PropertyChanged"/> event, and allows cancellation through the <see cref="CanSetProperty{T}"/> event.
        /// Outputs the old value of the property before it was changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the property's backing field.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property being updated. This is optional and can be automatically provided by the compiler.</param>
        /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
        /// <returns>True if the property value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, string? propertyName, out T oldValue)
        {
            oldValue = storage;
            if (EqualityComparer<T>.Default.Equals(storage, value) || !CanSetProperty(oldValue, value, propertyName))
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