#if NETFRAMEWORK || WINDOWS

using System;
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Defines a service interface for controlling the visibility and state of a window.
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// Gets the Window associated with this service.
        /// </summary>
        Window? Window { get; }

        /// <summary>
        /// Gets a value indicating whether the window is closed.
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Gets a value indicating whether the window is visible.
        /// </summary>
        bool IsVisible { get; }

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

        /// <summary>
        /// Minimizes the window.
        /// </summary>
        void Minimize();

        /// <summary>
        /// Maximizes the window.
        /// </summary>
        void Maximize();

        /// <summary>
        /// Restores the window to its normal state.
        /// </summary>
        void Restore();
    }

    /// <summary>
    /// Provides a service for interacting with a Window associated with a FrameworkElement.
    /// </summary>
    public class WindowService : WindowServiceBase, IWindowService
    {
        private bool? _isClosed;

        #region Properties

        /// <inheritdoc />
        public bool IsClosed => _isClosed == true;

        /// <inheritdoc />
        public bool IsVisible => Window?.IsVisible == true;

        #endregion

        #region Event Handlers

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            _isClosed = true;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Activate()
        {
            if (_isClosed == true)
            {
                return;
            }

            try
            {
                if (!IsVisible)
                {
                    Window?.Show();
                }
                Window?.Activate();
            }
            catch (InvalidOperationException)
            {
                _isClosed = true;
            }
        }

        /// <inheritdoc />
        public void Close()
        {
            if (_isClosed == true)
            {
                return;
            }
            Window?.Close();
        }

        /// <inheritdoc />
        public void Hide()
        {
            if (_isClosed == true)
            {
                return;
            }

            try
            {
                Window?.Hide();
            }
            catch (InvalidOperationException)
            {
                _isClosed = true;
            }
        }

        /// <inheritdoc />
        public void Show()
        {
            if (_isClosed == true)
            {
                return;
            }

            try
            {
                Window?.Show();
            }
            catch (InvalidOperationException)
            {
                _isClosed = true;
            }
        }

        /// <inheritdoc />
        public void Minimize()
        {
            if (_isClosed == true)
            {
                return;
            }
            if (Window != null)
            {
                Window.WindowState = WindowState.Minimized;
            }
        }

        /// <inheritdoc />
        public void Maximize()
        {
            if (_isClosed == true)
            {
                return;
            }
            if (Window != null)
            {
                Window.WindowState = WindowState.Maximized;
            }
        }

        /// <inheritdoc />
        public void Restore()
        {
            if (_isClosed == true)
            {
                return;
            }
            if (Window != null)
            {
                Window.WindowState = WindowState.Normal;
            }
        }

        protected override void OnWindowChanged(Window? oldWindow, Window? newWindow)
        {
            if (oldWindow != null)
            {
                oldWindow.Closed -= OnWindowClosed;
            }
            _isClosed = null;
            if (newWindow != null)
            {
                newWindow.Closed += OnWindowClosed;
            }

            base.OnWindowChanged(oldWindow, newWindow);
        }

        #endregion
    }
}
#endif
