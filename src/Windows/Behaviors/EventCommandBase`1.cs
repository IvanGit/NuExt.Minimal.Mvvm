#if NETFRAMEWORK || WINDOWS
using System.Windows;
using System.Windows.Input;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents an abstract base class for event triggers that execute commands in response to events.
    /// </summary>
    /// <typeparam name="T">The type of object to which the trigger is attached.</typeparam>
    public abstract class EventCommandBase<T> : EventTriggerBase<T> where T : DependencyObject
    {
        #region Dependency Properties

        /// <summary>
        /// Identifies the Command dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            nameof(Command), typeof(ICommand), typeof(EventCommandBase<T>));

        /// <summary>
        /// Identifies the CommandParameter dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            nameof(CommandParameter), typeof(object), typeof(EventCommandBase<T>));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the command to execute when the event is raised.
        /// </summary>
        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the parameter to pass to the command when it is executed.
        /// </summary>
        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the command can be executed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The event data.</param>
        /// <returns>true if the command can be executed; otherwise, false.</returns>
        protected virtual bool CanExecuteCommand(object? sender, object? eventArgs)
        {
            return IsEnabled;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The event data.</param>
        protected virtual void ExecuteCommand(object? sender, object? eventArgs)
        {
            var command = Command;
            if (command == null || !CanExecuteCommand(sender, eventArgs))
            {
                return;
            }
            var commandParameter = CommandParameter;
            if (command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }
        }

        /// <summary>
        /// Called when the associated event is raised.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The event data.</param>
        protected override void OnEvent(object? sender, object? eventArgs)
        {
            ExecuteCommand(sender, eventArgs);
        }

        #endregion
    }
}
#endif
