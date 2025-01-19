#if NETFRAMEWORK || WINDOWS
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a base class for services that can be attached to a FrameworkElement and need access to the Window.
    /// </summary>
    public abstract class WindowServiceBase : ServiceBase<FrameworkElement>
    {
        #region Dependency Properties

        private static readonly DependencyPropertyKey WindowPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Window), typeof(Window), typeof(WindowServiceBase),
            new PropertyMetadata(null, (d, e) => ((WindowServiceBase)d).OnWindowChanged((Window?)e.OldValue)));
        public static readonly DependencyProperty WindowProperty = WindowPropertyKey.DependencyProperty;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Window associated with this service.
        /// </summary>
        public Window? Window
        {
            get => (Window?)GetValue(WindowProperty);
            private set => SetValue(WindowPropertyKey, value);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when the Window associated with this service changes.
        /// Override this method to implement custom handling logic.
        /// </summary>
        /// <param name="oldWindow">The previous Window instance.</param>
        protected virtual void OnWindowChanged(Window? oldWindow)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the behavior is attached to an AssociatedObject.
        /// Sets the Window property to the Window containing the AssociatedObject.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            Window = AssociatedObject as Window ?? Window.GetWindow(AssociatedObject!);
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject.
        /// Clears the Window property.
        /// </summary>
        protected override void OnDetaching()
        {
            Window = null;
            base.OnDetaching();
        }

        #endregion
    }
}
#endif
