#if NETFRAMEWORK || WINDOWS
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Animation;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a base class that can be used to attach behaviors to WPF elements.
    /// </summary>
    public abstract class Behavior: Animatable, INotifyPropertyChanged
    {
        private readonly Type _associatedType;
        private DependencyObject? _associatedObject;

        #region Dependency Properties

        /// <summary>
        /// Identifies the IsEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            nameof(IsEnabled), typeof(bool), typeof(Behavior), new PropertyMetadata(true));

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Behavior"/> class with the specified type.
        /// </summary>
        /// <param name="associatedType">The type of object to which this behavior can be attached.</param>
        /// <exception cref="ArgumentNullException">Thrown when associatedType is null.</exception>
        protected Behavior(Type associatedType)
        {
            _associatedType = associatedType ?? throw new ArgumentNullException(nameof(associatedType));
        }

        #region Properties

        /// <summary>
        /// Gets the object to which this behavior is attached.
        /// </summary>
        public DependencyObject? AssociatedObject
        {
            get
            {
                ReadPreamble();
                return _associatedObject;
            }
            private set
            {
                if (AssociatedObject == value) return;
                WritePreamble();
                _associatedObject = value;
                WritePostscript();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the type of object to which this behavior can be attached.
        /// </summary>
        protected Type AssociatedType
        {
            get
            {
                ReadPreamble();
                return _associatedType;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this behavior is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Attaches the behavior to the specified object.
        /// </summary>
        /// <param name="obj">The object to attach to.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the behavior is already attached to an object, or when the type of the specified object does not match the associated type.
        /// </exception>
        public void Attach(DependencyObject? obj)
        {
            if (obj == AssociatedObject)
            {
                return;
            }
            Debug.Assert(AssociatedObject == null);
            ThrowInvalidOperationExceptionIfAttached();
            ThrowInvalidOperationExceptionIfTypeMismatch(obj?.GetType());
            AssociatedObject = obj;
            Debug.Assert(obj != null);
            if (AssociatedObject == null)
            {
                return;
            }
            OnAttached();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Behavior"/> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="Behavior"/> class.</returns>
        protected override Freezable? CreateInstanceCore()
        {
            return (Freezable?)Activator.CreateInstance(GetType());
        }

        /// <summary>
        /// Detaches the behavior from the associated object.
        /// </summary>
        public void Detach()
        {
            Debug.Assert(AssociatedObject != null);
            if (AssociatedObject == null)
            {
                return;
            }
            OnDetaching();
            AssociatedObject = null;
        }

        /// <summary>
        /// Called after the behavior is attached to an object.
        /// Override this method to hook up functionality to the associated object.
        /// </summary>
        protected virtual void OnAttached()
        {
        }

        /// <summary>
        /// Called before the behavior is detached from an object.
        /// Override this method to clean up any functionality hooked up in OnAttached.
        /// </summary>
        protected virtual void OnDetaching()
        {
        }

        /// <summary>
        /// Throws an InvalidOperationException if the behavior is already attached to an object.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the behavior is already attached to an object.</exception>
        private void ThrowInvalidOperationExceptionIfAttached()
        {
            if (AssociatedObject != null)
            {
                throw new InvalidOperationException($"An instance of a Behavior {GetType().Name} cannot be attached to more than one object at a time.");
            }
        }

        /// <summary>
        /// Throws an InvalidOperationException if the type of the specified object does not match the associated type.
        /// </summary>
        /// <param name="type">The type of the object to check.</param>
        /// <exception cref="InvalidOperationException">Thrown when the type of the specified object does not match the associated type.</exception>
        private void ThrowInvalidOperationExceptionIfTypeMismatch(Type? type)
        {
            if (type != null && !AssociatedType.IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"Cannot attach type {GetType().Name} to type {type.Name}. Instances of type {GetType().Name} can only be attached to objects of type {AssociatedType.Name}.");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that changed. This optional parameter 
        /// can be automatically provided by the CallerMemberName attribute.
        /// </param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
#endif
