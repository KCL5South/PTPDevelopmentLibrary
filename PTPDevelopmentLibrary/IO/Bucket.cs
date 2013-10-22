using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
namespace PTPDevelopmentLibrary.IO
{
    /// <summary>
    /// The Bucket object is meant to be represent and be used as a backing store for streams.
    /// The Bucket object takes in a generic stream and in the background loads the entire content of that stream
    /// into it's "bucket".  Meanwhile, multiple <see cref="BucketStream"/> object can read from the bucket.
    /// </summary>
    /// <remarks>Use the <see cref="M:Bucket.CreateReader"/> method to create multiple streams to speed up the parsing of large files.</remarks>
    public sealed class Bucket
    {
        /// <summary>
        /// The Resource Stop is a utility object definition, used to control thread syncronization.
        /// </summary>
        private class ResourceStop
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="readOffset">The offset into the stream that reading is requested to start.</param>
            public ResourceStop(long readOffset)
            {
                ReadOffset = readOffset;
                ResetEvent = new AutoResetEvent(false);
            }

            /// <summary>
            /// Gets the desired offset
            /// </summary>
            public long ReadOffset { get; private set; }
            /// <summary>
            /// Gets the <see cref="AutoResetEvent"/> wait handle used to syncronize threads.
            /// </summary>
            public AutoResetEvent ResetEvent { get; private set; }
        }

        /// <summary>
        /// The Resource Stop Collection is a utility object definition, used to control thread syncronization.
        /// </summary>
        private class ResourcesStopCollection : Collection<ResourceStop>
        {
            private Bucket _bucket;
            private object _syncObject;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="bucket">The <see cref="Bucket"/> object to work with.</param>
            public ResourcesStopCollection(Bucket bucket)
            {
                if (bucket == null)
                    throw new ArgumentNullException("bucket");

                _bucket = bucket;
                _syncObject = new object();
                _bucket.ProgressChanged += Update;
                _bucket.FinishedLoading += Finish;
            }

            private void Update(double progress)
            {
                lock (_syncObject)
                {
                    foreach (ResourceStop stop in (from rs in this
                                                   where rs.ReadOffset < _bucket.CurrentLength
                                                   select rs).ToArray())
                        this.Remove(stop);
                }
            }

            private void Finish()
            {
                lock (_syncObject)
                {
                    foreach (ResourceStop stop in this.ToArray())
                        this.Remove(stop);
                }
            }

            /// <summary>
            /// Call this method to syncronize a read at the offset represented by <paramref name="offset"/>.
            /// </summary>
            /// <param name="offset">The position in the stream to start the read.</param>
            /// <param name="timeout">The amount of time to wait for the syncronization. -1 = indefinite</param>
            public void Wait(long offset, int timeout)
            {
                if (_bucket._curLength > offset || _bucket.IsFinishedLoading)
                    return;
                else
                {
                    ResourceStop stop = null;

                    lock (_syncObject)
                    {
                        stop = new ResourceStop(offset);
                        this.Add(stop);
                    }

                    stop.ResetEvent.WaitOne(timeout);
                }
            }

            /// <summary>
            /// Takes care of the logic to remove the <see cref="ResourceStop"/> object from the underlying store, and
            /// to signal the thread syncronization.
            /// </summary>
            /// <param name="index">The position of the item to remove.</param>
            protected override void RemoveItem(int index)
            {
                ResourceStop target = null;
                lock (_syncObject) { target = this[index]; }
                target.ResetEvent.Set();
                //target.ResetEvent.Dispose();

                base.RemoveItem(index);
            }
        }

        private Stream _target;
        private long _length;
        private long _curLength;
        private byte[] _buffer;
        private bool _isFinished;
        private Thread _backgroundThread;
        private object _syncObject;
        private ResourcesStopCollection _stops;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target">The stream to read in.</param>
        /// <param name="onProgressChanged">A delegate used to signal progress.</param>
        public Bucket(Stream target, Action<double> onProgressChanged = null)
        {
            if (!target.CanSeek)
                throw new ArgumentException("The supplied stream must support seeking.  Otherwise, the length of the stream needs to be supplied.", "target");

            if (onProgressChanged != null)
                ProgressChanged += onProgressChanged;

            Initialize(target, target.Length);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target">The stream to read in.</param>
        /// <param name="targetLength">The overall length of the stream.</param>
        /// <param name="onProgressChanged">A delegate used to signal progress.</param>
        public Bucket(Stream target, long targetLength, Action<double> onProgressChanged = null)
        {
            if (onProgressChanged != null)
                ProgressChanged += onProgressChanged;

            Initialize(target, targetLength);
        }

        /// <summary>
        /// Gets a value signifying the final status of the bucket.
        /// </summary>
        public bool IsFinishedLoading
        {
            get
            {
                return _isFinished;
            }
            private set
            {
                if (_isFinished != value)
                {
                    _isFinished = value;
                    if (FinishedLoading != null)
                        FinishedLoading();
                }
            }
        }

        /// <summary>
        /// Gets a value representing the number of bytes that have been succesfully writen to the bucket.
        /// </summary>
        public long CurrentLength
        {
            get
            {
                return _curLength;
            }
        }

        /// <summary>
        /// Gets a value representing the total number of bytes that will be read from the stream.
        /// </summary>
        public long Length
        {
            get
            {
                return _length;
            }
        }

        /// <summary>
        /// Reads <paramref name="count"/> bytes from the bucket and writes them to <paramref name="buffer"/> starting
        /// at the position represented by <paramref name="bufferOffset"/>.
        /// </summary>
        /// <param name="buffer">The destination of the requested bytes.</param>
        /// <param name="bufferOffset">The offset within <paramref name="buffer"/> to start writing.</param>
        /// <param name="readOffset">The offset within the bucket to start reading.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <param name="timeout">The amount of time in milliseconds to wait for the read to finish.</param>
        /// <returns>The number of bytes actually read.  This value may be less than <paramref name="count"/> given
        /// how many bytes are available to read.  A return value of zero represnts the end of the bucket.
        /// (Bottom of the bucket.)</returns>
        public int Read(byte[] buffer, long bufferOffset, long readOffset, int count, int timeout)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (bufferOffset < 0)
                throw new ArgumentOutOfRangeException("bufferOffset", "offset is negative.");
            if(count < 0)
                throw new ArgumentOutOfRangeException("count", "count is negative");
            if (readOffset < 0)
                throw new ArgumentOutOfRangeException("readOffset", "readOffset is negative");
            if (buffer.Length < bufferOffset + count)
                throw new ArgumentException("The sum of offset and count is larger than the buffer length.");

            _stops.Wait(readOffset, timeout);

            long currentLength = _curLength;

            //After
            if (currentLength > readOffset + count)
            {
                lock (_syncObject)
                {
                    Array.Copy(_buffer, readOffset, buffer, bufferOffset, count);
                }
                return count;
            }
            //In Between
            else if (currentLength > readOffset && currentLength <= readOffset + count)
            {
                lock (_syncObject)
                {
                    Array.Copy(_buffer, readOffset, buffer, bufferOffset, currentLength - readOffset);
                }
                return (int)(currentLength - readOffset);
            }
            else if (currentLength == Length)
                return 0;
            //Before
            else
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Unknown Read Error: bufferOffset = {0}, readOffset = {1}, count = {2}, timeout = {3}", bufferOffset, readOffset, count, timeout));
                System.Diagnostics.Trace.TraceError(string.Format("Unknown Read Error: bufferOffset = {0}, readOffset = {1}, count = {2}, timeout = {3}", bufferOffset, readOffset, count, timeout));
                throw new IOException("Unexpected IO error from the StreamBucket");
            }
        }

        /// <summary>
        /// Returns a <see cref="Stream"/> that can be used to read from the bucket immediatly.
        /// </summary>
        /// <returns>A <see cref="Stream"/>.</returns>
        public Stream CreateReader()
        {
            return new BucketStream(this);
        }

        /// <summary>
        /// This even fires whenever the <see cref="P:CurrentLength"/> property changes.
        /// </summary>
        public event Action<double> ProgressChanged;

        /// <summary>
        /// This event fires when the bucket is finished loading from the backing stream.
        /// </summary>
        public event Action FinishedLoading;

        private void Initialize(Stream target, long targetLength)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (!target.CanRead)
                throw new ArgumentException("The target stream must be readable.", "target");
            if (targetLength < 0)
                throw new InvalidOperationException("target length must be greater than zero.");

            _target = target;
            _length = targetLength;
            _buffer = new byte[_length];
            _isFinished = false;
            _syncObject = new object();
            _stops = new ResourcesStopCollection(this);

            _backgroundThread = new Thread(new ParameterizedThreadStart(PopulateBuffer));
            _backgroundThread.Name = "StreamBuffer - Populating";
            _backgroundThread.Start(this);
        }

        private void OnProgressChanged()
        {
            if (ProgressChanged != null)
                ProgressChanged((double)CurrentLength / (double)Length);
        }

        #region Background Logic

        private static readonly int InternalBufferLength = 4096;

        private static void PopulateBuffer(object buffer)
        {
            Bucket streamBuffer = buffer as Bucket;
            if(streamBuffer == null)
                return;

            try
            {
                byte[] internalBuffer = new byte[InternalBufferLength];
                int readBytes = 0;
                while ((readBytes = streamBuffer._target.Read(internalBuffer, 0, InternalBufferLength)) > 0)
                {
                    lock (streamBuffer._syncObject)
                    {
                        Array.Copy(internalBuffer, 0, streamBuffer._buffer, streamBuffer._curLength, readBytes);
                        streamBuffer._curLength += readBytes;
                    }
                    streamBuffer.OnProgressChanged();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("StreamBucket._backgroundThread({0}) has encountered an error: {1}", streamBuffer._backgroundThread.ManagedThreadId, ex.Message));
                System.Diagnostics.Trace.TraceError(string.Format("StreamBucket._backgroundThread({0}) has encountered an error: {1}", streamBuffer._backgroundThread.ManagedThreadId, ex.Message));
            }
            finally
            {
                streamBuffer.IsFinishedLoading = true;
            }

            System.Diagnostics.Debug.WriteLine("StreamBucket.PopulateBuffer finished running.");
        }

        #endregion
    }
}