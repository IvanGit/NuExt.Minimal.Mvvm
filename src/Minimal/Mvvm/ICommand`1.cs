using System.Windows.Input;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Defines a command that can be executed with a parameter of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public interface ICommand<in T> : ICommand
    {
        /// <summary>
        /// Executes the command with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the command.</param>
        void Execute(T parameter);

        /// <summary>
        /// Determines whether the command can execute with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to evaluate for determining if the command can execute.</param>
        /// <returns>True if the command can execute; otherwise, false.</returns>
        bool CanExecute(T parameter);
    }
}
