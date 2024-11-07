#if NETFRAMEWORK || WINDOWS
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents an event trigger that can execute a command in response to an event.
    /// This class extends EventCommandBase and adds support for converting event arguments
    /// before passing them to the command and optionally passing the event arguments directly.
    /// </summary>
    public class EventTrigger : EventCommandBase<DependencyObject>
    {
        #region Dependency Properties

        /// <summary>
        /// Identifies the EventArgsConverter dependency property.
        /// Allows specifying a converter to convert event arguments before passing them to the command.
        /// </summary>
        public static readonly DependencyProperty EventArgsConverterProperty = DependencyProperty.Register(
            nameof(EventArgsConverter), typeof(IValueConverter), typeof(EventTrigger));

        /// <summary>
        /// Identifies the PassEventArgsToCommand dependency property.
        /// Specifies whether to pass the event arguments directly to the command.
        /// </summary>
        public static readonly DependencyProperty PassEventArgsToCommandProperty = DependencyProperty.Register(
            nameof(PassEventArgsToCommand), typeof(bool?), typeof(EventTrigger));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the converter used to convert event arguments before passing them to the command.
        /// </summary>
        public IValueConverter? EventArgsConverter
        {
            get => (IValueConverter?)GetValue(EventArgsConverterProperty);
            set => SetValue(EventArgsConverterProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to pass the event arguments directly to the command.
        /// </summary>
        public bool? PassEventArgsToCommand
        {
            get => (bool?)GetValue(PassEventArgsToCommandProperty);
            set => SetValue(PassEventArgsToCommandProperty, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the associated command with the appropriate parameters, either converting the
        /// event arguments using the specified converter or passing the event arguments directly
        /// if configured to do so.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The event data.</param>
        protected override void ExecuteCommand(object? sender, object? eventArgs)
        {
            var command = Command;
            if (command == null || !CanExecuteCommand(sender, eventArgs))
            {
                return;
            }
            var commandParameter = CommandParameter;
            if (commandParameter == null)
            {
                if (EventArgsConverter != null)
                {
                    commandParameter = EventArgsConverter.Convert(eventArgs, typeof(object), sender, CultureInfo.CurrentCulture);
                }
                else if (PassEventArgsToCommand == true)
                {
                    commandParameter = eventArgs;
                }
            }
            if (command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }
        }

        /// <summary>
        /// Invoked when the associated event is raised. Ensures that the command is executed
        /// even if the binding has not yet been evaluated by using the dispatcher to delay execution.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The event data.</param>
        protected override void OnEvent(object? sender, object? eventArgs)
        {
            if (Command == null && BindingOperations.GetBindingExpression(this, CommandProperty) != null)
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    ExecuteCommand(sender, eventArgs);
                }));
                return;
            }
            ExecuteCommand(sender, eventArgs);
        }

        #endregion
    }
}
#endif