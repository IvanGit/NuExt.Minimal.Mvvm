#if NETFRAMEWORK || WINDOWS
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a base class for services that can be attached to a FrameworkElement.
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
        /// Identifies the Name dependency property.
        /// </summary>
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            nameof(Name), typeof(string), typeof(ServiceBase<T>));

        #endregion

        #region Properties

        /// <summary>
        /// The name of the service.
        /// </summary>
        public string Name
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the service should register itself in the ViewModel.
        /// Default value is true.
        /// </summary>
        public bool ShouldRegisterInViewModel { get; set; } = true;

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
                newViewModel.Services.RegisterService((object)this, Name, true);
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

        #endregion
    }
}
#endif