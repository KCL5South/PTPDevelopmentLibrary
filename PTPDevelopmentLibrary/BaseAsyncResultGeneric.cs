using System.Threading;
using System;
namespace PTPDevelopmentLibrary
{
    /// <summary>
    /// A type safe version of <see cref="BaseAsyncResult"/>.
    /// </summary>
    /// <seealso cref="BaseAsyncResult"/>
    /// <typeparam name="T">The type of the operation result.</typeparam>
    public abstract class BaseAsyncResult<T> : BaseAsyncResult
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cb">The method that will be called when the async operation completes.</param>
        /// <param name="state">A user defined state object.</param>
        public BaseAsyncResult(AsyncCallback cb, object state)
            : base(cb, state)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cb">The method that will be called when the async operation completes.</param>
        /// <param name="state">A user defined state object.</param>
        /// <param name="completed">A value indicating wether the async operation has completed syncronously.</param>
        public BaseAsyncResult(AsyncCallback cb, object state, bool completed)
            : base(cb, state, completed)
        {
        }

        /// <summary>
        /// Gets the Result of the asyncronous operation.
        /// </summary>
        public new T Result
        {
            get
            {
                return (T)base.Result;
            }
        }
    }
}