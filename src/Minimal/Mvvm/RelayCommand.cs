using System;

namespace Minimal.Mvvm
{
    /// <summary>
    /// A command that relays its functionality to other objects by invoking delegates.
    /// This is a non-generic version of <see cref="RelayCommand{T}"/> for commands that do not require parameters.
    /// Supports multiple execution.
    /// </summary>
    public class RelayCommand: RelayCommand<object?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <exception cref="ArgumentNullException">Thrown if the execute argument is null.</exception>
        public RelayCommand(Action execute) : this(execute, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic. If null, the command can always execute.</param>
        /// <exception cref="ArgumentNullException">Thrown if the execute argument is null.</exception>
        public RelayCommand(Action execute, Func<bool>? canExecute) : base(_ => execute(), canExecute != null ? _ => canExecute() : null)
        {
        }
    }
}
