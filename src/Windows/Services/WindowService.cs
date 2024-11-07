#if NETFRAMEWORK || WINDOWS

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Defines a service interface for controlling the visibility and state of a window.
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// Activates the window, bringing it to the foreground.
        /// </summary>
        void Activate();

        /// <summary>
        /// Closes the window.
        /// </summary>
        void Close();

        /// <summary>
        /// Hides the window.
        /// </summary>
        void Hide();

        /// <summary>
        /// Shows the window.
        /// </summary>
        void Show();
    }

    /// <summary>
    /// Provides a service for interacting with a Window associated with a FrameworkElement.
    /// </summary>
    public class WindowService : WindowServiceBase, IWindowService
    {
        #region Methods

        /// <summary>
        /// Activates the associated Window and brings it to the foreground.
        /// </summary>
        public void Activate()
        {
            Window?.Activate();
        }

        /// <summary>
        /// Closes the associated Window.
        /// </summary>
        public void Close()
        {
            Window?.Close();
        }

        /// <summary>
        /// Hides the associated Window.
        /// </summary>
        public void Hide()
        {
            Window?.Hide();
        }

        /// <summary>
        /// Shows the associated Window.
        /// </summary>
        public void Show()
        {
            Window?.Show();
        }

        #endregion
    }
}
#endif
