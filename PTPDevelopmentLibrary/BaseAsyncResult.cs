using System.Threading;
using System;
namespace PTPDevelopmentLibrary
{
    /// <summary>
    /// This object represents a base asyncronous result.
    /// </summary>
    /// <remarks>
    /// Inherit from this object to define a custom async result.
    /// </remarks>
    /// <seealso cref="BaseAsyncResult{T}"/>
    public abstract class BaseAsyncResult : IAsyncResult, IDisposable
    {
        private readonly AsyncCallback callback_;
        private bool completed_;
        private bool completedSynchronously_;
        private readonly object asyncState_;
        private readonly ManualResetEvent waitHandle_;
        private object result_;
        private Exception e_;
        private readonly object syncRoot_;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cb">The method that will be called when the async operation completes.</param>
        /// <param name="state">A user defined state object.</param>
        public BaseAsyncResult(AsyncCallback cb, object state)
            : this(cb, state, false)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cb">The method that will be called when the async operation completes.</param>
        /// <param name="state">A user defined state object.</param>
        /// <param name="completed">A value indicating wether the async operation has completed syncronously.</param>
        public BaseAsyncResult(AsyncCallback cb, object state, bool completed)
        {
            this.callback_ = cb;
            this.asyncState_ = state;
            this.completed_ = completed;
            this.completedSynchronously_ = completed;

            this.waitHandle_ = new ManualResetEvent(false);
            this.syncRoot_ = new object();
        }

        #region IAsyncResult Members

        /// <summary>
        /// Gets the user defined state object.
        /// </summary>
        public object AsyncState
        {
            get { return this.asyncState_; }
        }
        /// <summary>
        /// Gets a <see cref="System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get { return this.waitHandle_; }
        }
        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation completed synchronously.
        /// </summary>
        public bool CompletedSynchronously
        {
            get
            {
                lock (this.syncRoot_)
                {
                    return this.completedSynchronously_;
                }
            }
        }
        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                lock (this.syncRoot_)
                {
                    return this.completed_;
                }
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Cleans up resources allocated by this object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Overriding this method will allow special logic to be executed when this async result is disposed.
        /// </summary>
        /// <param name="disposing">A value indicating wether the async result is being disposed or not.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this.syncRoot_)
                {
                    if (this.waitHandle_ != null)
                    {
                        ((IDisposable)this.waitHandle_).Dispose();
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets an <see cref="Exception"/> if one was encountered during the asnycronous operation.
        /// </summary>
        public Exception Exception
        {
            get
            {
                lock (this.syncRoot_)
                {
                    return this.e_;
                }
            }
        }

        /// <summary>
        /// Gets the result of the asyncronous operation.
        /// </summary>
        public object Result
        {
            get
            {
                lock (this.syncRoot_)
                {
                    return this.result_;
                }
            }
        }

        /// <summary>
        /// Call this method to signal the completion of the asyncronous operation.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="completedSynchronously">A value indicating wether the operation completed syncronously or not.</param>
        public void Complete(object result, bool completedSynchronously)
        {
            lock (this.syncRoot_)
            {
                this.completed_ = true;
                this.completedSynchronously_ =
                    completedSynchronously;
                this.result_ = result;
            }

            this.SignalCompletion();
        }

        /// <summary>
        /// Call this method to signal an exception within the asyncronous operation.
        /// </summary>
        /// <param name="e">The <see cref="Exception"/> that was encountered.</param>
        /// <param name="completedSynchronously">A value indicating wether the operation completed syncronously or not.</param>
        public void HandleException(Exception e, bool completedSynchronously)
        {
            lock (this.syncRoot_)
            {
                this.completed_ = true;
                this.completedSynchronously_ =
                    completedSynchronously;
                this.e_ = e;
            }

            this.SignalCompletion();
        }

        private void SignalCompletion()
        {
            this.waitHandle_.Set();

            ThreadPool.QueueUserWorkItem(new WaitCallback(this.InvokeCallback));
        }

        private void InvokeCallback(object state)
        {
            if (this.callback_ != null)
            {
                this.callback_(this);
            }
        }
    }
}