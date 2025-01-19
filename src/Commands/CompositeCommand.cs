using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Represents a command that aggregates multiple commands and executes them sequentially.
    /// </summary>
    public class CompositeCommand : ICommand, IDisposable
    {
        private readonly IEnumerable<ICommand> _commands;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeCommand"/> class with the specified commands.
        /// </summary>
        /// <param name="commands">The collection of commands to be aggregated.</param>
        /// <exception cref="ArgumentNullException">Thrown when the commands parameter is null.</exception>
        public CompositeCommand(IEnumerable<ICommand> commands)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            SubscribeToCommands();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeCommand"/> class with the specified commands.
        /// </summary>
        /// <param name="commands">The array of commands to be aggregated.</param>
        /// <exception cref="ArgumentNullException">Thrown when the commands parameter is null.</exception>
        public CompositeCommand(params ICommand[] commands) : this((IEnumerable<ICommand>)commands)
        {

        }

        #region Events

        private event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        event EventHandler? ICommand.CanExecuteChanged
        {
            add => CanExecuteChanged += value;
            remove => CanExecuteChanged -= value;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the CanExecuteChanged event of the individual commands.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OnCommandCanExecuteChanged(object? sender, EventArgs e)
        {
            RaiseCanExecuteChanged();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether all aggregated commands can execute in their current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if all commands can execute; otherwise, false.</returns>
        public bool CanExecute(object? parameter)
        {
            bool canExecute = false;
            foreach (var command in _commands)
            {
                if (!command.CanExecute(parameter))
                {
                    return false;
                }
                canExecute = true;
            }
            return canExecute;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="CompositeCommand"/> instance.
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromCommands();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Executes all aggregated commands in sequence.
        /// </summary>
        /// <param name="parameter">Data used by the commands. If the commands do not require data to be passed, this object can be set to null.</param>
        public void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }
            foreach (var command in _commands)
            {
                command.Execute(parameter);
            }
        }

        /// <summary>
        /// Raises the CanExecuteChanged event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Subscribes to the CanExecuteChanged events of the aggregated commands.
        /// </summary>
        private void SubscribeToCommands()
        {
            foreach (var command in _commands)
            {
                command.CanExecuteChanged += OnCommandCanExecuteChanged;
            }
        }

        /// <summary>
        /// Unsubscribes from the CanExecuteChanged events of the aggregated commands.
        /// </summary>
        private void UnsubscribeFromCommands()
        {
            foreach (var command in _commands)
            {
                command.CanExecuteChanged -= OnCommandCanExecuteChanged;
            }
        }

        #endregion
    }
}
