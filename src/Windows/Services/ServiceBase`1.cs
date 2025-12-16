#if NETFRAMEWORK || WINDOWS
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a base class for services that can be attached to a FrameworkElement.
    /// Provides automatic registration with the ViewModel's service container when the DataContext changes.
    /// </summary>
    /// <typeparam name="T">The type of the FrameworkElement to which this service is attached.</typeparam>
    [RuntimeNameProperty(nameof(Name))]
    public abstract class ServiceBase<T> : Behavior<T> where T : FrameworkElement
    {
        #region Dependency Properties

        /// <summary>
        /// Identifies the DataContext dependency property.
        /// </summary>
        private static readonly DependencyProperty DataContextProperty = DependencyProperty.Register(
            nameof(FrameworkElement.DataContext), typeof(object), typeof(ServiceBase<T>),
            new PropertyMetadata(null, (d, e) => ((ServiceBase<T>)d).OnDataContextChanged(e.OldValue, e.NewValue)));

        /// <summary>
        /// Identifies the <see cref="Name"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            nameof(Name), typeof(string), typeof(ServiceBase<T>),
            new PropertyMetadata(null, (d, e) => ((ServiceBase<T>)d).OnNameChanged((string?)e.OldValue, (string?)e.NewValue)));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        public string Name
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        private bool _shouldRegisterInViewModel = true;
        /// <summary>
        /// Gets or sets a value indicating whether the service should register itself in the ViewModel.
        /// Default value is <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property should be set during initialization (typically in XAML) and not changed afterwards.
        /// </para>
        /// <para>
        /// Changing this property while the service is attached has no effect.
        /// </para>
        /// </remarks>
        public bool ShouldRegisterInViewModel 
        {
            get => _shouldRegisterInViewModel;
            set
            {
                if (IsAttached) return;
                _shouldRegisterInViewModel = value;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when the DataContext of the associated object changes.
        /// Registers or unregisters the service in the ViewModel as needed.
        /// </summary>
        /// <param name="oldDataContext">The old DataContext value.</param>
        /// <param name="newDataContext">The new DataContext value.</param>
        protected virtual void OnDataContextChanged(object? oldDataContext, object? newDataContext)
        {
            if (!ShouldRegisterInViewModel)
            {
                return;
            }
            if (oldDataContext is ViewModelBase oldViewModel)
            {
                oldViewModel.Services.UnregisterService(this);
            }
            if (newDataContext is ViewModelBase newViewModel)
            {
                newViewModel.Services.RegisterService(GetType(), this, Name, true);
            }
        }

        /// <summary>
        /// Called when the <see cref="Name"/> property changes.
        /// Registers or unregisters the service in the ViewModel as needed.
        /// </summary>
        /// <param name="oldName">The old Name value.</param>
        /// <param name="newName">The new Name value.</param>
        protected virtual void OnNameChanged(string? oldName, string? newName)
        {
            if (!ShouldRegisterInViewModel)
            {
                return;
            }
            if (string.Equals(oldName, newName, StringComparison.Ordinal))
            {
                return;
            }
            if (GetValue(DataContextProperty) is ViewModelBase viewModel)
            {
                viewModel.Services.UnregisterService(this);
                viewModel.Services.RegisterService(GetType(), this, newName, true);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the behavior is attached to an AssociatedObject.
        /// Sets up a binding to the DataContext of the associated FrameworkElement.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            if (!ShouldRegisterInViewModel)
            {
                return;
            }
            BindingOperations.SetBinding(this, DataContextProperty, new Binding()
            {
                Source = AssociatedObject,
                Path = new PropertyPath(FrameworkElement.DataContextProperty),
                Mode = BindingMode.OneWay
            });
        }

        /// <summary>
        /// Called before the behavior is detached from an AssociatedObject.
        /// Clean up a binding to the DataContext of the associated FrameworkElement.
        /// </summary>
        protected override void OnDetaching()
        {
            BindingOperations.ClearBinding(this, DataContextProperty);

            base.OnDetaching();
        }

        #endregion
    }
}
#endif