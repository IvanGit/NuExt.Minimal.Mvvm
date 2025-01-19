using System;
using System.Threading.Tasks;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Represents an asynchronous command that does not take any parameters.
    /// Supports multiple execution.
    /// </summary>
    public class AsyncCommand : AsyncCommand<object?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommand"/> class with the specified execution logic.
        /// </summary>
        /// <param name="execute">The asynchronous execution logic of the command.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="execute"/> is null.</exception>
        public AsyncCommand(Func<Task> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommand"/> class with the specified execution and canExecute logic.
        /// </summary>
        /// <param name="execute">The asynchronous execution logic of the command.</param>
        /// <param name="canExecute">The logic that determines whether the command can execute. If null, the command is always enabled.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="execute"/> is null.</exception>
        public AsyncCommand(Func<Task> execute, Func<bool>? canExecute) : base(_ => execute(), canExecute != null ? _ => canExecute() : null)
        {
        }
    }
}
