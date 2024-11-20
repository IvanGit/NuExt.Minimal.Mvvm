using System.Threading;
using System.Threading.Tasks;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Interface for an asynchronous command with a parameter of type T, extending 
    /// the functionalities of <see cref="ICommand{T}"/> and <see cref="IAsyncCommand"/>.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public interface IAsyncCommand<in T>: ICommand<T>, IAsyncCommand
    {
        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter to be used by the command.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(T parameter);

        /// <summary>
        /// Executes the command asynchronously with cancellation support.
        /// </summary>
        /// <param name="parameter">The parameter to be used by the command.</param>
        /// <param name="cancellationToken">A cancellation token for managing the command execution.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(T parameter, CancellationToken cancellationToken);
    }
}
