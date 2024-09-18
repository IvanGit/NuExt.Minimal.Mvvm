using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Minimal.Mvvm
{
    /// <summary>
    /// Base class for ViewModels.
    /// </summary>
    public abstract class ViewModelBase : BindableBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        protected ViewModelBase()
        {
            Thread = Thread.CurrentThread;
        }

        #region Properties

        /// <summary>
        /// Gets the thread on which the current instance was created.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Thread Thread { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the current thread is the same as the thread on which this instance was created.
        /// </summary>
        /// <returns>True if the current thread is the same as the creation thread; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAccess()
        {
            return Thread == Thread.CurrentThread;
        }
        
        #endregion
    }

}
