#if NETFRAMEWORK || WINDOWS
using System.Windows;
using System.Windows.Input;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a trigger that executes a command in response to a specific key gesture.
    /// Inherits from EventCommandBase and is associated with UIElement's KeyUp event by default.
    /// </summary>
    public class KeyTrigger : EventCommandBase<UIElement>
    {
        #region Dependency Properties

        /// <summary>
        /// Identifies the Gesture dependency property.
        /// Allows specifying a key gesture that triggers the command.
        /// </summary>
        public static readonly DependencyProperty GestureProperty = DependencyProperty.Register(
            nameof(Gesture), typeof(KeyGesture), typeof(KeyTrigger));

        #endregion
        static KeyTrigger()
        {
            // Overrides the default event name to be "KeyUp".
            EventNameProperty.OverrideMetadata(typeof(KeyTrigger), new PropertyMetadata(nameof(UIElement.KeyUp)));
        }

        #region Properties

        /// <summary>
        /// Gets or sets the key gesture that triggers the command.
        /// </summary>
        public KeyGesture? Gesture
        {
            get => (KeyGesture?)GetValue(GestureProperty);
            set => SetValue(GestureProperty, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the command can be executed based on the provided event arguments.
        /// Checks if the specified key gesture matches the input event arguments.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The event data.</param>
        /// <returns>True if the command can be executed; otherwise, false.</returns>
        protected override bool CanExecuteCommand(object? sender, object? eventArgs)
        {
            if (!base.CanExecuteCommand(sender, eventArgs))
            {
                return false;
            }
            if (Gesture == null || eventArgs is not InputEventArgs inputEventArgs)
            {
                return false;
            }
            return Gesture.Matches(AssociatedObject, inputEventArgs);
        }

        #endregion
    }
}
#endif
